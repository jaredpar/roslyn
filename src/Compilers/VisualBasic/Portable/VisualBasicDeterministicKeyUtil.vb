' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic
    Friend Class VisualBasicDeterministicKeyUtil
        Inherits DeterministicKeyUtil

        Friend Shared Function GenerateKey(compilation As VisualBasicCompilation) As String
            ' PROTOTYPE: must append all vb specific compilation options
            Dim builder As New StringBuilder()
            AppendCommonCompilationOptions(builder, compilation.Options)
            AppendSyntaxTrees(builder, compilation.SyntaxTrees)
            AppendReferences(builder, compilation.References)
            Return builder.ToString()
        End Function

        Friend Shared Function GenerateKey(compilationData As CommonCompilationData) As String
            ' PROTOTYPE: must append all vb specific compilation options
            Dim builder As New StringBuilder()
            AppendCommonCompilationOptions(builder, CType(compilationData.CompilationOptions, VisualBasicCompilationOptions))
            AppendSourceFiles(builder, compilationData.SourceFiles)
            AppendReferences(builder, compilationData.References)
            Return builder.ToString()
        End Function
    End Class
End Namespace

