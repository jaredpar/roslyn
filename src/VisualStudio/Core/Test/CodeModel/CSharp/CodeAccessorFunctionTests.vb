' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Threading.Tasks
Imports Microsoft.CodeAnalysis
Imports Roslyn.Test.Utilities

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.CodeModel.CSharp
    Public Class CodeAccessorFunctionTests
        Inherits AbstractCodeFunctionTests

#Region "Access tests"

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestAccess1() As Task
            Dim code =
    <Code>
class C
{
    public int P
    {
        get
        {
            return 0;
        }
        $$set
        {
        }
    }
}
</Code>

            Await TestAccess(code, EnvDTE.vsCMAccess.vsCMAccessPublic)
        End Function

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestAccess2() As Task
            Dim code =
    <Code>
class C
{
    public int P
    {
        get
        {
            return 0;
        }
        private $$set
        {
        }
    }
}
</Code>

            Await TestAccess(code, EnvDTE.vsCMAccess.vsCMAccessPrivate)
        End Function

#End Region

        <ConditionalWpfFact(GetType(x86)), Trait(Traits.Feature, Traits.Features.CodeModel)>
        Public Async Function TestTypeDescriptor_GetProperties() As Task
            Dim code =
<Code>
class C
{
    int P { $$get { return 42; } }
}
</Code>

            Await TestPropertyDescriptors(Of EnvDTE80.CodeFunction2)(code)
        End Function

        Protected Overrides ReadOnly Property LanguageName As String
            Get
                Return LanguageNames.CSharp
            End Get
        End Property
    End Class
End Namespace


