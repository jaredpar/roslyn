// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    // PROTOTYPE: need to include file paths in the file here
    // PROTOTYPE: invariant that needs to be tested: CommonCompiler and Compilation should emit the same key
    // PROTOTYPE: need to handle mapped paths.
    // PROTOTYPE: deal with errors on parse options, and errors in general
    // start with CSharpCompilation and we can work backwards to avoiding a lot of the work like parsing
    // files.
    internal abstract class DeterministicKeyUtil
    {
        // PROTOTYPE DO we need the parse options at all or is the source file enough here?
        protected static void AppendCommonParseOptions(StringBuilder builder, ParseOptions parseOptions)
        {
            // PROTOTYPE: errors, features
            builder.AppendLine($"{nameof(ParseOptions.DocumentationMode)}-{parseOptions.DocumentationMode}");
            builder.AppendLine($"{nameof(ParseOptions.Kind)} - {parseOptions.Kind}");
            builder.AppendLine($"{nameof(ParseOptions.SpecifiedKind)} - {parseOptions.SpecifiedKind}");
            builder.AppendLine("Preprocessor Symbols");
            foreach (var name in parseOptions.PreprocessorSymbolNames)
            {
                builder.AppendLine(name);
            }
        }

        protected static void AppendCommonCompilationOptions(StringBuilder builder, CompilationOptions options)
        {
            // PROTOTYPE: crypto key file, script class name,
            builder.AppendLine($"Checkoverflow: {options.CheckOverflow}");
            builder.AppendLine($"Concurrent build: {options.ConcurrentBuild}");
            builder.AppendLine($"Key container: {options.CryptoKeyContainer}");
            builder.AppendLine($"Delaysign: {options.DelaySign}");
            builder.AppendLine($"Metadata import options: {options.MetadataImportOptions}");
            builder.AppendLine($"Module name: {options.ModuleName}");
            builder.AppendLine($"Optimization  level: {options.OptimizationLevel}");
            builder.AppendLine($"OutputKind: {options.OutputKind}");
            builder.AppendLine($"Platform: {options.Platform}");
            builder.AppendLine($"Publicsign: {options.PublicSign}");
            builder.AppendLine($"Warning level: {options.WarningLevel}");
        }

        protected static void AppendSyntaxTrees(StringBuilder builder, IEnumerable<SyntaxTree> syntaxTrees, CancellationToken cancellationToken = default)
        {
            builder.AppendLine("Source Files");
            foreach (var syntaxTree in syntaxTrees)
            {
                var sourceFile = syntaxTree.GetText(cancellationToken);
                var checksum = sourceFile.GetChecksum();
                var fileName = Path.GetFileName(syntaxTree.FilePath);
                builder.Append(fileName);
                builder.Append(": ");
                Append(builder, sourceFile.GetChecksum());
                builder.AppendLine();
            }
        }

        protected static void Append(StringBuilder builder, ImmutableArray<byte> bytes)
        {
            char getHexValue(int i) => i < 10
                ? (char)(i + '0')
                : (char)(i - 10 + 'A');

            foreach (var b in bytes)
            {
                builder.Append(getHexValue(b / 16));
                builder.Append(getHexValue(b % 16));
            }
        }

        protected static void AppendReferences(StringBuilder builder, IEnumerable<MetadataReference> references)
        {
            builder.AppendLine("References");
            foreach (var reference in references)
            {
                switch (reference)
                {
                    case MetadataImageReference mir:
                        appendMetadataImageReference(mir);
                        break;
                    default:
                        // PROTOTYPE: this can be extended by third parties hence can't exhaust all cases here. Possibly 
                        // add a GetDeterministicKey member to MetadataReference that defaults to Guid.NewGuid()
                        builder.AppendLine($"{reference}: {Guid.NewGuid()}");
                        break;
               }
            }

            void appendMetadataImageReference(MetadataImageReference r)
            {
                var name = Path.GetFileName(r.FilePath);
                var metadata = r.GetMetadata();
                switch (metadata)
                {
                    case AssemblyMetadata assemblyMetadata:
                        {
                            var modules = assemblyMetadata.GetModules();
                            Debug.Assert(!modules.IsDefaultOrEmpty);
                            builder.AppendLine($"{name}: {modules[0].GetModuleVersionId()}");
                            break;
                        }
                    case ModuleMetadata moduleMetadata:
                        {
                            builder.AppendLine($"{name}: {moduleMetadata.GetModuleVersionId()}");
                            break;
                        }
                    default:
                        throw ExceptionUtilities.UnexpectedValue(metadata);
               }
            }
        }

        internal static void EmitDeterminismKey(CommandLineArguments args, string[] rawArgs, string baseDirectory, CommandLineParser parser)
        {
            var key = CreateDeterminismKey(args, rawArgs, baseDirectory, parser);
            var filePath = Path.Combine(args.OutputDirectory, args.OutputFileName + ".key");
            using (var stream = File.Create(filePath))
            {
                var bytes = Encoding.UTF8.GetBytes(key);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// The string returned from this function represents the inputs to the compiler which impact determinism.  It is 
        /// meant to be inline with the specification here:
        /// 
        ///     - https://github.com/dotnet/roslyn/blob/master/docs/compilers/Deterministic%20Inputs.md
        /// 
        /// Issue #8193 tracks filling this out to the full specification. 
        /// 
        ///     https://github.com/dotnet/roslyn/issues/8193
        /// </summary>
        internal static string CreateDeterminismKey(CommandLineArguments args, string[] rawArgs, string baseDirectory, CommandLineParser parser)
        {
            List<Diagnostic> diagnostics = new List<Diagnostic>();
            List<string> flattenedArgs = new List<string>();
            parser.FlattenArgs(rawArgs, diagnostics, flattenedArgs, null, baseDirectory);

            var builder = new StringBuilder();
            var name = !string.IsNullOrEmpty(args.OutputFileName)
                ? Path.GetFileNameWithoutExtension(Path.GetFileName(args.OutputFileName))
                : $"no-output-name-{Guid.NewGuid().ToString()}";

            builder.AppendLine($"{name}");
            builder.AppendLine($"Command Line:");
            foreach (var current in flattenedArgs)
            {
                builder.AppendLine($"\t{current}");
            }

            builder.AppendLine("Source Files:");
            var hash = MD5.Create();
            foreach (var sourceFile in args.SourceFiles)
            {
                var sourceFileName = Path.GetFileName(sourceFile.Path);

                string hashValue;
                try
                {
                    var bytes = File.ReadAllBytes(sourceFile.Path);
                    var hashBytes = hash.ComputeHash(bytes);
                    var data = BitConverter.ToString(hashBytes);
                    hashValue = data.Replace("-", "");
                }
                catch (Exception ex)
                {
                    hashValue = $"Could not compute {ex.Message}";
                }
                builder.AppendLine($"\t{sourceFileName} - {hashValue}");
            }

            return builder.ToString();
        }
    }
}
