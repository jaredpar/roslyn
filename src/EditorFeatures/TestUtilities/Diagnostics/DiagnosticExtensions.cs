// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis
{
    public static class FeaturesDiagnosticExtensions
    {
        public static DiagnosticAnalyzer GetCompilerDiagnosticAnalyzer(string languageName)
        {
            return languageName == LanguageNames.CSharp ?
                (DiagnosticAnalyzer)new Diagnostics.CSharp.CSharpCompilerDiagnosticAnalyzer() :
                new Diagnostics.VisualBasic.VisualBasicCompilerDiagnosticAnalyzer();
        }

        public static ImmutableDictionary<string, ImmutableArray<DiagnosticAnalyzer>> GetCompilerDiagnosticAnalyzersMap()
        {
            var builder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<DiagnosticAnalyzer>>();
            builder.Add(LanguageNames.CSharp, ImmutableArray.Create(GetCompilerDiagnosticAnalyzer(LanguageNames.CSharp)));
            builder.Add(LanguageNames.VisualBasic, ImmutableArray.Create(GetCompilerDiagnosticAnalyzer(LanguageNames.VisualBasic)));
            return builder.ToImmutable();
        }

        public static AnalyzerReference GetCompilerDiagnosticAnalyzerReference(string languageName)
        {
            var analyzer = GetCompilerDiagnosticAnalyzer(languageName);
            return new AnalyzerImageReference(ImmutableArray.Create(analyzer), display: analyzer.GetType().FullName);
        }

        public static ImmutableDictionary<string, ImmutableArray<AnalyzerReference>> GetCompilerDiagnosticAnalyzerReferencesMap()
        {
            var builder = ImmutableDictionary.CreateBuilder<string, ImmutableArray<AnalyzerReference>>();
            builder.Add(LanguageNames.CSharp, ImmutableArray.Create(GetCompilerDiagnosticAnalyzerReference(LanguageNames.CSharp)));
            builder.Add(LanguageNames.VisualBasic, ImmutableArray.Create(GetCompilerDiagnosticAnalyzerReference(LanguageNames.VisualBasic)));
            return builder.ToImmutable();
        }
    }
}
