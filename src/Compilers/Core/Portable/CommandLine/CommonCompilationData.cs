using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis
{
    internal readonly struct CommonCompilationSourceFile
    {
        // PROTOTYPE: should not be using normalized path here. That is a C# only construct
        internal SourceText SourceText { get; }
        internal SyntaxTree SyntaxTree { get; }
        internal string NormalizedFilePath { get; }
        internal bool IsScript { get; }

        internal CommonCompilationSourceFile(
            SourceText sourceText,
            SyntaxTree syntaxTree,
            string normalizedFilePath,
            bool isScript)
        {
            Debug.Assert(sourceText != null);
            Debug.Assert(normalizedFilePath != null);
            SourceText = sourceText;
            SyntaxTree = syntaxTree;
            NormalizedFilePath = normalizedFilePath;
            IsScript = isScript;
        }

        internal CommonCompilationSourceFile WithSyntaxTree(SyntaxTree syntaxTree) =>
            new CommonCompilationSourceFile(
                SourceText,
                syntaxTree,
                NormalizedFilePath,
                IsScript);
    }

    internal readonly struct CommonCompilationData
    {
        internal CommonCompilationSourceFile[] SourceFiles { get; }
        internal List<MetadataReference> References { get; }
        internal CompilationOptions CompilationOptions { get; }

        internal bool HasSyntaxTrees => SourceFiles.Length > 0 && SourceFiles[0].SyntaxTree != null;

        internal CommonCompilationData(
            CommonCompilationSourceFile[] sourceFiles,
            List<MetadataReference> references,
            CompilationOptions options)
        {
            Debug.Assert(sourceFiles != null);
            Debug.Assert(references != null);
            Debug.Assert(options != null);
            SourceFiles = sourceFiles;
            References = references;
            CompilationOptions = options;
        }
    }
}
