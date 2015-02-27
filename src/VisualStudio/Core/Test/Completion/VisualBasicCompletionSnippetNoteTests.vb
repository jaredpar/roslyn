' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Completion.Providers
Imports Microsoft.CodeAnalysis.Editor.UnitTests.IntelliSense
Imports Microsoft.CodeAnalysis.Snippets
Imports Microsoft.CodeAnalysis.Text
Imports Roslyn.Test.Utilities

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.Completion
    Public Class VisualBasicCompletionSnippetNoteTests
        Private _markup As XElement = <document>
                                         <![CDATA[Imports System
Class Foo
    $$
End Class]]></document>

        <Fact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Sub SnippetExpansionNoteAddedToDescription_ExactMatch()
            Using state = CreateVisualBasicSnippetExpansionNoteTestState(_markup, "Interface")
                state.SendTypeChars("Interfac")
                state.AssertCompletionSession()
                state.AssertSelectedCompletionItem(description:="Interface Keyword" & vbCrLf &
                    "Declares the name of an interface and the definitions of the members of the interface." & vbCrLf &
                    "Note: Tab twice to insert the 'Interface' snippet.")
            End Using
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Sub SnippetExpansionNoteAddedToDescription_DifferentSnippetShortcutCasing()
            Using state = CreateVisualBasicSnippetExpansionNoteTestState(_markup, "intErfaCE")
                state.SendTypeChars("Interfac")
                state.AssertCompletionSession()
                state.AssertSelectedCompletionItem(description:="Interface Keyword" & vbCrLf &
                    "Declares the name of an interface and the definitions of the members of the interface." & vbCrLf &
                    "Note: Tab twice to insert the 'Interface' snippet.")
            End Using
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Sub SnippetExpansionNoteNotAddedToDescription_ShortcutIsProperSubstringOfInsertedText()
            Using state = CreateVisualBasicSnippetExpansionNoteTestState(_markup, "Interfac")
                state.SendTypeChars("Interfac")
                state.AssertCompletionSession()
                state.AssertSelectedCompletionItem(description:="Interface Keyword" & vbCrLf &
                    "Declares the name of an interface and the definitions of the members of the interface.")
            End Using
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Sub SnippetExpansionNoteNotAddedToDescription_ShortcutIsProperSuperstringOfInsertedText()
            Using state = CreateVisualBasicSnippetExpansionNoteTestState(_markup, "Interfaces")
                state.SendTypeChars("Interfac")
                state.AssertCompletionSession()
                state.AssertSelectedCompletionItem(description:="Interface Keyword" & vbCrLf &
                    "Declares the name of an interface and the definitions of the members of the interface.")
            End Using
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Sub SnippetExpansionNoteNotAddedToDescription_DisplayTextMatchesShortcutButInsertionTextDoesNot()
            Using state = CreateVisualBasicSnippetExpansionNoteTestState(_markup, "DisplayText")

                state.SendTypeChars("DisplayTex")
                state.AssertCompletionSession()
                state.AssertSelectedCompletionItem(description:="")
            End Using
        End Sub

        <Fact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Sub SnippetExpansionNoteAddedToDescription_DisplayTextDoesNotMatchShortcutButInsertionTextDoes()
            Using state = CreateVisualBasicSnippetExpansionNoteTestState(_markup, "InsertionText")

                state.SendTypeChars("DisplayTex")
                state.AssertCompletionSession()
                state.AssertSelectedCompletionItem(description:="Note: Tab twice to insert the 'InsertionText' snippet.")
            End Using
        End Sub

        Private Function CreateVisualBasicSnippetExpansionNoteTestState(xElement As XElement, ParamArray snippetShortcuts As String()) As TestState
            Dim state = TestState.CreateVisualBasicTestState(
                xElement,
                New ICompletionProvider() {New MockCompletionProvider(New TextSpan(31, 10))},
                Nothing,
                New List(Of Type) From {GetType(TestVisualBasicSnippetInfoService)})

            Dim testSnippetInfoService = DirectCast(state.Workspace.Services.GetLanguageServices(LanguageNames.VisualBasic).GetService(Of ISnippetInfoService)(), TestVisualBasicSnippetInfoService)
            testSnippetInfoService.SetSnippetShortcuts(snippetShortcuts)

            Return state
        End Function
    End Class
End Namespace
