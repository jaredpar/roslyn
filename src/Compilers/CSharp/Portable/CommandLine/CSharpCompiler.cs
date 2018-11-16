// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp
{
    internal abstract class CSharpCompiler : CommonCompiler
    {
        internal const string ResponseFileName = "csc.rsp";

        private readonly CommandLineDiagnosticFormatter _diagnosticFormatter;
        private readonly string _tempDirectory;

        protected CSharpCompiler(CSharpCommandLineParser parser, string responseFile, string[] args, BuildPaths buildPaths, string additionalReferenceDirectories, IAnalyzerAssemblyLoader assemblyLoader)
            : base(parser, responseFile, args, buildPaths, additionalReferenceDirectories, assemblyLoader)
        {
            _diagnosticFormatter = new CommandLineDiagnosticFormatter(buildPaths.WorkingDirectory, Arguments.PrintFullPaths, Arguments.ShouldIncludeErrorEndLocation);
            _tempDirectory = buildPaths.TempDirectory;
        }

        public override DiagnosticFormatter DiagnosticFormatter { get { return _diagnosticFormatter; } }
        protected internal new CSharpCommandLineArguments Arguments { get { return (CSharpCommandLineArguments)base.Arguments; } }

        public override CommonCompilationData? CreateCompilationData(TextWriter consoleOutput, TouchedFileLogger touchedFilesLogger, ErrorLogger errorLogger, bool delayParse)
        {
            var parseOptions = Arguments.ParseOptions;

            // We compute script parse options once so we don't have to do it repeatedly in
            // case there are many script files.
            var scriptParseOptions = parseOptions.WithKind(SourceCodeKind.Script);

            bool hadErrors = false;

            var sourceFiles = new CommonCompilationSourceFile?[Arguments.SourceFiles.Length - 1];
            var diagnosticBag = DiagnosticBag.GetInstance();

            if (Arguments.CompilationOptions.ConcurrentBuild)
            {
                Parallel.For(0, sourceFiles.Length, UICultureUtilities.WithCurrentUICulture<int>(i =>
                {
                    //NOTE: order of trees is important!!
                    sourceFiles[i] = parseFile(Arguments.SourceFiles[i], ref hadErrors);
                }));
            }
            else
            {
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    //NOTE: order of trees is important!!
                    sourceFiles[i] = parseFile(Arguments.SourceFiles[i], ref hadErrors);
                }
            }

            // If errors had been reported in ParseFile, while trying to read files, then we should simply exit.
            if (hadErrors)
            {
                Debug.Assert(diagnosticBag.HasAnyErrors());
                ReportErrors(diagnosticBag.ToReadOnlyAndFree(), consoleOutput, errorLogger);
                return null;
            }

            Debug.Assert(diagnosticBag.IsEmptyWithoutResolution);
            diagnosticBag.Free();

            var diagnostics = new List<DiagnosticInfo>();
            var uniqueFilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < sourceFiles.Length; i++)
            {
                Debug.Assert(sourceFiles[i].HasValue);
                var normalizedFilePath = sourceFiles[i].Value.NormalizedFilePath;
                Debug.Assert(normalizedFilePath != null);
                Debug.Assert(PathUtilities.IsAbsolute(normalizedFilePath));

                if (!uniqueFilePaths.Add(normalizedFilePath))
                {
                    // warning CS2002: Source file '{0}' specified multiple times
                    diagnostics.Add(new DiagnosticInfo(MessageProvider, (int)ErrorCode.WRN_FileAlreadyIncluded,
                        Arguments.PrintFullPaths ? normalizedFilePath : _diagnosticFormatter.RelativizeNormalizedPath(normalizedFilePath)));

                    sourceFiles[i] = null;
                }
            }

            if (Arguments.TouchedFilesPath != null)
            {
                foreach (var path in uniqueFilePaths)
                {
                    touchedFilesLogger.AddRead(path);
                }
            }

            var assemblyIdentityComparer = DesktopAssemblyIdentityComparer.Default;
            var appConfigPath = this.Arguments.AppConfigPath;
            if (appConfigPath != null)
            {
                try
                {
                    using (var appConfigStream = new FileStream(appConfigPath, FileMode.Open, FileAccess.Read))
                    {
                        assemblyIdentityComparer = DesktopAssemblyIdentityComparer.LoadFromXml(appConfigStream);
                    }

                    if (touchedFilesLogger != null)
                    {
                        touchedFilesLogger.AddRead(appConfigPath);
                    }
                }
                catch (Exception ex)
                {
                    diagnostics.Add(new DiagnosticInfo(MessageProvider, (int)ErrorCode.ERR_CantReadConfigFile, appConfigPath, ex.Message));
                }
            }

            var xmlFileResolver = new LoggingXmlFileResolver(Arguments.BaseDirectory, touchedFilesLogger);
            var sourceFileResolver = new LoggingSourceFileResolver(ImmutableArray<string>.Empty, Arguments.BaseDirectory, Arguments.PathMap, touchedFilesLogger);

            MetadataReferenceResolver referenceDirectiveResolver;
            var resolvedReferences = ResolveMetadataReferences(diagnostics, touchedFilesLogger, out referenceDirectiveResolver);
            if (ReportErrors(diagnostics, consoleOutput, errorLogger))
            {
                return null;
            }

            var loggingFileSystem = new LoggingStrongNameFileSystem(touchedFilesLogger, _tempDirectory);

            return new CommonCompilationData(
                sourceFiles.Where(x => x.HasValue).Select(x => x.Value).ToArray(),
                resolvedReferences,
                Arguments.CompilationOptions.
                    WithMetadataReferenceResolver(referenceDirectiveResolver).
                    WithAssemblyIdentityComparer(assemblyIdentityComparer).
                    WithXmlReferenceResolver(xmlFileResolver).
                    WithStrongNameProvider(Arguments.GetStrongNameProvider(loggingFileSystem)).
                    WithSourceReferenceResolver(sourceFileResolver));

            CommonCompilationSourceFile parseFile(
                CommandLineSourceFile file,
                ref bool addedDiagnostics)
            {
                var fileDiagnostics = new List<DiagnosticInfo>();
                var content = TryReadFileContent(file, fileDiagnostics, out string normalizedFilePath);

                if (content == null)
                {
                    foreach (var info in fileDiagnostics)
                    {
                        diagnosticBag.Add(MessageProvider.CreateDiagnostic(info));
                    }
                    fileDiagnostics.Clear();
                    addedDiagnostics = true;
                }

                var sourceFile = new CommonCompilationSourceFile(content, syntaxTree: null, normalizedFilePath, file.IsScript);
                if (!delayParse)
                {
                    Debug.Assert(fileDiagnostics.Count == 0);
                    var syntaxTree = ParseFile(parseOptions, scriptParseOptions, sourceFile);
                    sourceFile = sourceFile.WithSyntaxTree(syntaxTree);
                }

                return sourceFile;
            }
        }

        public override Compilation CreateCompilation(CommonCompilationData compilationData)
        {
            IEnumerable<SyntaxTree> syntaxTrees;
            if (compilationData.HasSyntaxTrees)
            {
                syntaxTrees = compilationData.SourceFiles.Select(x => x.SyntaxTree);
            }
            else
            {
                var sourceFiles = compilationData.SourceFiles;
                var parseOptions = Arguments.ParseOptions;
                var scriptParseOptions = parseOptions.WithKind(SourceCodeKind.Script);

                var trees = new SyntaxTree[compilationData.SourceFiles.Length];
                if (compilationData.CompilationOptions.ConcurrentBuild)
                {
                    Parallel.For(0, sourceFiles.Length, UICultureUtilities.WithCurrentUICulture<int>(i =>
                    {
                        //NOTE: order of trees is important!!
                        trees[i] = ParseFile(parseOptions, scriptParseOptions, sourceFiles[i]);
                    }));
                }
                else
                {
                    for (int i = 0; i < sourceFiles.Length; i++)
                    {
                        //NOTE: order of trees is important!!
                        trees[i] = ParseFile(parseOptions, scriptParseOptions, sourceFiles[i]);
                    }
                }

                syntaxTrees = trees;
            }

            return CSharpCompilation.Create(
                Arguments.CompilationName,
                syntaxTrees.WhereNotNull(),
                compilationData.References,
                (CSharpCompilationOptions)(compilationData.CompilationOptions));
        }

        private static SyntaxTree ParseFile(
            CSharpParseOptions parseOptions,
            CSharpParseOptions scriptParseOptions,
            CommonCompilationSourceFile content)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
                content.SourceText,
                content.IsScript ? scriptParseOptions : parseOptions,
                content.NormalizedFilePath);

            // prepopulate line tables.
            // we will need line tables anyways and it is better to not wait until we are in emit
            // where things run sequentially.
            bool isHiddenDummy;
            tree.GetMappedLineSpanAndVisibility(default(TextSpan), out isHiddenDummy);

            return tree;
        }

        /// <summary>
        /// Given a compilation and a destination directory, determine three names:
        ///   1) The name with which the assembly should be output.
        ///   2) The path of the assembly/module file.
        ///   3) The path of the pdb file.
        ///
        /// When csc produces an executable, but the name of the resulting assembly
        /// is not specified using the "/out" switch, the name is taken from the name
        /// of the file (note: file, not class) containing the assembly entrypoint
        /// (as determined by binding and the "/main" switch).
        ///
        /// For example, if the command is "csc /target:exe a.cs b.cs" and b.cs contains the
        /// entrypoint, then csc will produce "b.exe" and "b.pdb" in the output directory,
        /// with assembly name "b" and module name "b.exe" embedded in the file.
        /// </summary>
        protected override string GetOutputFileName(Compilation compilation, CancellationToken cancellationToken)
        {
            if (Arguments.OutputFileName == null)
            {
                Debug.Assert(Arguments.CompilationOptions.OutputKind.IsApplication());

                var comp = (CSharpCompilation)compilation;

                Symbol entryPoint = comp.ScriptClass;
                if ((object)entryPoint == null)
                {
                    var method = comp.GetEntryPoint(cancellationToken);
                    if ((object)method != null)
                    {
                        entryPoint = method.PartialImplementationPart ?? method;
                    }
                }

                if ((object)entryPoint != null)
                {
                    string entryPointFileName = PathUtilities.GetFileName(entryPoint.Locations.First().SourceTree.FilePath);
                    return Path.ChangeExtension(entryPointFileName, ".exe");
                }
                else
                {
                    // no entrypoint found - an error will be reported and the compilation won't be emitted
                    return "error";
                }
            }
            else
            {
                return base.GetOutputFileName(compilation, cancellationToken);
            }
        }

        public override bool TrypGetDeterministicKey(CommonCompilationData compilationData, out string key)
        {
            key = CSharpDeterministicKeyUtil.GenerateKey(compilationData);
            return true;
        }

        internal override bool SuppressDefaultResponseFile(IEnumerable<string> args)
        {
            return args.Any(arg => new[] { "/noconfig", "-noconfig" }.Contains(arg.ToLowerInvariant()));
        }

        /// <summary>
        /// Print compiler logo
        /// </summary>
        /// <param name="consoleOutput"></param>
        public override void PrintLogo(TextWriter consoleOutput)
        {
            consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine1, Culture), GetToolName(), GetAssemblyFileVersion());
            consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LogoLine2, Culture));
            consoleOutput.WriteLine();
        }

        public override void PrintLangVersions(TextWriter consoleOutput)
        {
            consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_LangVersions, Culture));
            var defaultVersion = LanguageVersion.Default.MapSpecifiedToEffectiveVersion();
            var latestVersion = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();
            foreach (LanguageVersion v in Enum.GetValues(typeof(LanguageVersion)))
            {
                if (v == defaultVersion)
                {
                    consoleOutput.WriteLine($"{v.ToDisplayString()} (default)");
                }
                else if (v == latestVersion)
                {
                    consoleOutput.WriteLine($"{v.ToDisplayString()} (latest)");
                }
                else if (v == LanguageVersion.CSharp8)
                {
                    // https://github.com/dotnet/roslyn/issues/29819 This should be removed once we are ready to move C# 8.0 out of beta
                    consoleOutput.WriteLine($"{v.ToDisplayString()} *beta*");
                }
                else
                {
                    consoleOutput.WriteLine(v.ToDisplayString());
                }
            }
            consoleOutput.WriteLine();
        }

        internal override Type Type
        {
            get
            {
                // We do not use this.GetType() so that we don't break mock subtypes
                return typeof(CSharpCompiler);
            }
        }

        internal override string GetToolName()
        {
            return ErrorFacts.GetMessage(MessageID.IDS_ToolName, Culture);
        }

        /// <summary>
        /// Print Commandline help message (up to 80 English characters per line)
        /// </summary>
        /// <param name="consoleOutput"></param>
        public override void PrintHelp(TextWriter consoleOutput)
        {
            consoleOutput.WriteLine(ErrorFacts.GetMessage(MessageID.IDS_CSCHelp, Culture));
        }

        protected override bool TryGetCompilerDiagnosticCode(string diagnosticId, out uint code)
        {
            return CommonCompiler.TryGetCompilerDiagnosticCode(diagnosticId, "CS", out code);
        }

        protected override ImmutableArray<DiagnosticAnalyzer> ResolveAnalyzersFromArguments(
            List<DiagnosticInfo> diagnostics,
            CommonMessageProvider messageProvider)
        {
            return Arguments.ResolveAnalyzersFromArguments(LanguageNames.CSharp, diagnostics, messageProvider, AssemblyLoader);
        }

        protected override void ResolveEmbeddedFilesFromExternalSourceDirectives(
            SyntaxTree tree,
            SourceReferenceResolver resolver,
            OrderedSet<string> embeddedFiles,
            DiagnosticBag diagnostics)
        {
            foreach (LineDirectiveTriviaSyntax directive in tree.GetRoot().GetDirectives(
                d => d.IsActive && !d.HasErrors && d.Kind() == SyntaxKind.LineDirectiveTrivia))
            {
                string path = (string)directive.File.Value;
                if (path == null)
                {
                    continue;
                }

                string resolvedPath = resolver.ResolveReference(path, tree.FilePath);
                if (resolvedPath == null)
                {
                    diagnostics.Add(
                        MessageProvider.CreateDiagnostic(
                            (int)ErrorCode.ERR_NoSourceFile,
                            directive.File.GetLocation(),
                            path,
                            CSharpResources.CouldNotFindFile));

                    continue;
                }

                embeddedFiles.Add(resolvedPath);
            }
        }
    }
}
