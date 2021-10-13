﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// The string returned from this function represents the inputs to the compiler which impact determinism.  It is 
    /// meant to be inline with the specification here:
    /// 
    ///     - https://github.com/dotnet/roslyn/blob/main/docs/compilers/Deterministic%20Inputs.md
    /// 
    /// Issue #8193 tracks filling this out to the full specification. 
    /// 
    ///     https://github.com/dotnet/roslyn/issues/8193
    /// </summary>
    /// <remarks>
    /// Options which can cause compilation failure, but doesn't impact the result of a successful
    /// compilation should be included. That is because it is interesting to describe error states
    /// not just success states. Think about caching build failures as well as build successes
    ///
    /// API considerations
    /// - Path dependent
    /// - throw when using a non-deterministic compilation
    /// </remarks>
    internal abstract class DeterministicKeyBuilder
    {
        protected DeterministicKeyBuilder()
        {
        }

        protected void WriteFileName(JsonWriter writer, string name, string? filePath, DeterministicKeyOptions options)
        {
            if (0 != (options & DeterministicKeyOptions.IgnorePaths))
            {
                filePath = Path.GetFileName(filePath);
            }

            writer.Write(name, filePath);
        }

        protected void WriteByteArrayValue(JsonWriter writer, string name, ImmutableArray<byte> value)
        {
            if (!value.IsDefault)
            {
                WriteByteArrayValue(writer, name, value.AsSpan());
            }
        }

        internal static void EncodeByteArrayValue(ReadOnlySpan<byte> value, StringBuilder builder)
        {
            foreach (var b in value)
            {
                builder.Append(b.ToString("x"));
            }
        }

        protected void WriteByteArrayValue(JsonWriter writer, string name, ReadOnlySpan<byte> value)
        {
            var builder = PooledStringBuilder.GetInstance();
            EncodeByteArrayValue(value, builder.Builder);
            writer.Write(name, builder.ToStringAndFree());
        }

        private (JsonWriter, PooledStringBuilder) CreateWriter()
        {
            var builder = PooledStringBuilder.GetInstance();
            var writer = new StringWriter(builder);
            return (new JsonWriter(writer), builder);
        }

        internal string GetKey(
            Compilation compilation,
            ImmutableArray<AdditionalText> additionalTexts = default,
            ImmutableArray<DiagnosticAnalyzer> analyzers = default,
            ImmutableArray<ISourceGenerator> generators = default,
            EmitOptions? emitOptions = null,
            DeterministicKeyOptions options = default)
        {
            ensureNotDefault(ref additionalTexts);
            ensureNotDefault(ref analyzers);
            ensureNotDefault(ref generators);

            var (writer, builder) = CreateWriter();

            writer.WriteObjectStart();

            writer.WriteKey("compilation");
            WriteCompilation(writer, compilation, options);
            writer.WriteKey("additionalTexts");
            writeAdditionalTexts();
            writer.WriteKey("analyzers");
            writeAnalyzers();
            writer.WriteKey("generators");
            writeGenerators();
            writer.WriteKey("emitOptions");
            WriteEmitOptions(writer, emitOptions);

            writer.WriteObjectEnd();

            return builder.ToStringAndFree();

            void writeAdditionalTexts()
            {
                writer.WriteArrayStart();
                foreach (var additionalText in additionalTexts)
                {
                    writer.WriteObjectStart();
                    WriteFileName(writer, "fileName", additionalText.Path, options);
                    writer.WriteKey("text");
                    WriteSourceText(writer, additionalText.GetText());
                    writer.WriteObjectEnd();
                }
                writer.WriteArrayEnd();
            }

            void writeAnalyzers()
            {
                writer.WriteArrayStart();
                foreach (var analyzer in analyzers)
                {
                    writeType(analyzer.GetType());
                }
                writer.WriteArrayEnd();
            }

            void writeGenerators()
            {
                writer.WriteArrayStart();
                foreach (var generator in generators)
                {
                    writeType(generator.GetType());
                }
                writer.WriteArrayEnd();
            }

            void writeType(Type type)
            {
                writer.WriteObjectStart();
                writer.Write("fullName", type.FullName);
                // Note that the file path to the assembly is deliberately not included here. The file path
                // of the assembly does not contribute to the output of the program.
                writer.Write("assemblyName", type.Assembly.FullName);
                writer.WriteObjectEnd();
            }

            void ensureNotDefault<T>(ref ImmutableArray<T> array)
            {
                if (array.IsDefault)
                {
                    array = ImmutableArray<T>.Empty;
                }
            }
        }

        internal string GetKey(EmitOptions? emitOptions)
        {
            var (writer, builder) = CreateWriter();
            WriteEmitOptions(writer, emitOptions);
            return builder.ToStringAndFree();
        }

        private void WriteCompilation(JsonWriter writer, Compilation compilation, DeterministicKeyOptions options)
        {
            writer.WriteObjectStart();
            writeToolsVersions();

            writer.WriteKey("options");
            WriteCompilationOptions(writer, compilation.Options);

            writer.WriteKey("syntaxTrees");
            writer.WriteArrayStart();
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                WriteSyntaxTree(writer, syntaxTree, options);
            }
            writer.WriteArrayEnd();

            writer.WriteKey("references");
            writer.WriteArrayStart();
            foreach (var reference in compilation.References)
            {
                WriteMetadataReference(writer, reference);
            }
            writer.WriteArrayEnd();
            writer.WriteObjectEnd();

            void writeToolsVersions()
            {
                writer.WriteKey("toolsVersions");
                writer.WriteObjectStart();
                if (0 != (options & DeterministicKeyOptions.IgnoreToolVersions))
                {
                    writer.WriteObjectEnd();
                    return;
                }

                var compilerVersion = typeof(Compilation).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                writer.Write("compilerVersion", compilerVersion);

                var runtimeVersion = typeof(object).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                writer.Write("runtimeVersion", runtimeVersion);

                writer.Write("framework", RuntimeInformation.FrameworkDescription);
                writer.Write("os", RuntimeInformation.OSDescription);

                writer.WriteObjectEnd();
            }
        }

        private void WriteSyntaxTree(JsonWriter writer, SyntaxTree syntaxTree, DeterministicKeyOptions options)
        {
            writer.WriteObjectStart();
            WriteFileName(writer, "fileName", syntaxTree.FilePath, options);
            writer.WriteKey("text");
            WriteSourceText(writer, syntaxTree.GetText());
            writer.WriteKey("parseOptions");
            WriteParseOptions(writer, syntaxTree.Options);
            writer.WriteObjectEnd();
        }

        private void WriteSourceText(JsonWriter writer, SourceText? sourceText)
        {
            if (sourceText is null)
            {
                return;
            }

            writer.WriteObjectStart();
            WriteByteArrayValue(writer, "checksum", sourceText.GetChecksum());
            writer.Write("checksumAlgorithm", sourceText.ChecksumAlgorithm);
            writer.Write("encoding", sourceText.Encoding?.EncodingName);
            writer.WriteObjectEnd();
        }

        internal void WriteMetadataReference(JsonWriter writer, MetadataReference reference)
        {
            writer.WriteObjectStart();
            if (reference is PortableExecutableReference peReference)
            {
                ModuleMetadata moduleMetadata;
                switch (peReference.GetMetadata())
                {
                    case AssemblyMetadata assemblyMetadata:
                        {
                            if (assemblyMetadata.GetModules() is { Length: 1 } modules)
                            {
                                moduleMetadata = modules[0];
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }
                        break;
                    case ModuleMetadata m:
                        moduleMetadata = m;
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                // The path of a reference, unlike the path of a file, does not contribute to the output
                // of the copmilation. Only the MVID, name and version contribute here hence the file path
                // is deliberately omitted here.
                if (moduleMetadata.GetMetadataReader() is { IsAssembly: true } peReader)
                {
                    var assemblyDef = peReader.GetAssemblyDefinition();
                    writer.Write("name", peReader.GetString(assemblyDef.Name));
                    writer.Write("version", assemblyDef.Version.ToString());
                    WriteByteArrayValue(writer, "publicKey", peReader.GetBlobBytes(assemblyDef.PublicKey).AsSpan());
                }

                writer.Write("mvid", moduleMetadata.GetModuleVersionId().ToString());
                writer.WriteKey("properties");
                writeMetadataReferenceProperties(writer, reference.Properties);
            }
            else
            {
                throw new InvalidOperationException();
            }
            writer.WriteObjectEnd();

            static void writeMetadataReferenceProperties(JsonWriter writer, MetadataReferenceProperties properties)
            {
                writer.WriteObjectStart();
                writer.Write("kind", properties.Kind);
                writer.Write("embedInteropTypes", properties.EmbedInteropTypes);
                if (properties.Aliases is { Length: > 0 } aliases)
                {
                    writer.WriteKey("aliases");
                    writer.WriteArrayStart();
                    foreach (var alias in aliases)
                    {
                        writer.Write(alias);
                    }
                    writer.WriteArrayEnd();
                }
                writer.WriteObjectEnd();
            }
        }

        private void WriteEmitOptions(JsonWriter writer, EmitOptions? options)
        {
            writer.WriteObjectStart();
            if (options is null)
            {
                writer.WriteObjectEnd();
                return;
            }

            writer.Write("emitMetadataOnly", options.EmitMetadataOnly);
            writer.Write("tolerateErrors", options.TolerateErrors);
            writer.Write("includePrivateMembers", options.IncludePrivateMembers);
            if (options.InstrumentationKinds.Length > 0)
            {
                writer.WriteArrayStart();
                foreach (var kind in options.InstrumentationKinds)
                {
                    writer.Write(kind);
                }
                writer.WriteArrayEnd();
            }

            writeSubsystemVersion(writer, options.SubsystemVersion);
            writer.Write("fileAlignment", options.FileAlignment);
            writer.Write("highEntropyVirtualAddressSpace", options.HighEntropyVirtualAddressSpace);
            writer.Write("baseAddress", options.BaseAddress.ToString());
            writer.Write("debugInformationFormat", options.DebugInformationFormat);
            writer.Write("outputNameOverride", options.OutputNameOverride);
            writer.Write("pdbFilePath", options.PdbFilePath);
            writer.Write("pdbChecksumAlgorithm", options.PdbChecksumAlgorithm.Name);
            writer.Write("runtimeMetadataVersion", options.RuntimeMetadataVersion);
            writer.Write("defaultSourceFileEncoding", options.DefaultSourceFileEncoding?.CodePage);
            writer.Write("fallbackSourceFileEncoding", options.FallbackSourceFileEncoding?.CodePage);

            writer.WriteObjectEnd();

            static void writeSubsystemVersion(JsonWriter writer, SubsystemVersion version)
            {
                writer.WriteKey("subsystemVersion");
                writer.WriteObjectStart();
                writer.Write("major", version.Major);
                writer.Write("minor", version.Minor);
                writer.WriteObjectEnd();
            }
        }

        private void WriteCompilationOptions(JsonWriter writer, CompilationOptions options)
        {
            writer.WriteObjectStart();
            WriteCompilationOptionsCore(writer, options);
            writer.WriteObjectEnd();
        }

        protected virtual void WriteCompilationOptionsCore(JsonWriter writer, CompilationOptions options)
        {
            // CompilationOption values
            writer.Write("outputKind", options.OutputKind);
            writer.Write("moduleName", options.ModuleName);
            writer.Write("scriptClassName", options.ScriptClassName);
            writer.Write("mainTypeName", options.MainTypeName);
            WriteByteArrayValue(writer, "cryptoPublicKey", options.CryptoPublicKey);
            writer.Write("cryptoKeyFile", options.CryptoKeyFile);
            writer.Write("delaySign", options.DelaySign);
            writer.Write("publicSign", options.PublicSign);
            writer.Write("checkOverflow", options.CheckOverflow);
            writer.Write("platform", options.Platform);
            writer.Write("optimizationLevel", options.OptimizationLevel);
            writer.Write("generalDiagnosticOption", options.GeneralDiagnosticOption);
            writer.Write("warningLevel", options.WarningLevel);
            writer.Write("deterministic", options.Deterministic);
            writer.Write("debugPlusMode", options.DebugPlusMode);
            writer.Write("referencesSupersedeLowerVersions", options.ReferencesSupersedeLowerVersions);
            writer.Write("reportSuppressedDiagnostics", options.ReportSuppressedDiagnostics);
            writer.Write("nullableContextOptions", options.NullableContextOptions);

            writer.WriteKey("specificDiagnosticOptions");
            writer.WriteArrayStart();
            foreach (var kvp in options.SpecificDiagnosticOptions)
            {
                writer.WriteObjectStart();
                writer.Write(kvp.Key, kvp.Value);
                writer.WriteObjectEnd();
            }
            writer.WriteArrayEnd();

            // Skipped values
            // - ConcurrentBuild
            // - CurrentLocalTime: this is only valid when Determinism is false at which point the key isn't
            //   valid
            // - MetadataImportOptions: does not impact compilation success or failure
            // - Options.Features: deprecated
            // 
            // Not really options, implementation details that can't really be expressed in a key
            // - SyntaxTreeOptionsProvider 
            // - MetadataReferenceResolver 
            // - XmlReferenceResolver
            // - SourceReferenceResolver
            // - StrongNameProvider
            //
            // Think harder about 
            // - AssemblyIdentityComparer
        }

        protected void WriteParseOptions(JsonWriter writer, ParseOptions parseOptions)
        {
            writer.WriteObjectStart();
            WriteParseOptionsCore(writer, parseOptions);
            writer.WriteObjectEnd();
        }

        protected virtual void WriteParseOptionsCore(JsonWriter writer, ParseOptions parseOptions)
        {
            writer.Write("kind", parseOptions.Kind);
            writer.Write("specifiedKind", parseOptions.SpecifiedKind);
            writer.Write("documentationMode", parseOptions.DocumentationMode);
            writer.Write("language", parseOptions.Language);

            var features = parseOptions.Features;
            writer.WriteKey("features");
            writer.WriteArrayStart();
            foreach (var kvp in features)
            {
                writer.WriteObjectStart();
                writer.Write(kvp.Key, kvp.Value);
                writer.WriteObjectEnd();
            }
            writer.WriteArrayEnd();

            // Skipped values
            // - Errors: not sure if we need that in the key file or not
            // - PreprocessorSymbolNames: handled at the language specific level
        }
    }
}
