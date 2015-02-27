' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Namespace Microsoft.CodeAnalysis.VisualBasic.EditAndContinue
    Friend Partial Class StatementSyntaxComparer
        ''' <summary>
        ''' Compares nodes that have a single body.
        ''' </summary>
        Friend NotInheritable Class SingleBody
            Inherits StatementSyntaxComparer

            Friend Shared ReadOnly [Default] As StatementSyntaxComparer = New SingleBody(Nothing, Nothing)

            Private ReadOnly _oldRoot As SyntaxNode
            Private ReadOnly _newRoot As SyntaxNode

            Friend Sub New(oldRoot As SyntaxNode, newRoot As SyntaxNode)
                Me._oldRoot = oldRoot
                Me._newRoot = newRoot
            End Sub

            Private Function HasChildren(node As SyntaxNode) As Boolean
                ' Root is classified as leaf by default, since we don't want to descend into it while matching the parent body.
                Return NonRootHasChildren(node) OrElse node Is _oldRoot OrElse node Is _newRoot
            End Function

            Protected Overrides Function GetChildren(node As SyntaxNode) As IEnumerable(Of SyntaxNode)
                Debug.Assert(GetLabel(node) <> IgnoredNode)
                Return If(HasChildren(node), EnumerateChildren(node), Nothing)
            End Function

            Protected Overrides Iterator Function GetDescendants(node As SyntaxNode) As IEnumerable(Of SyntaxNode)
                For Each descendant In node.DescendantNodesAndTokens(
                    descendIntoChildren:=AddressOf HasChildren,
                    descendIntoTrivia:=False)

                    Dim descendantNode = descendant.AsNode()
                    If descendantNode IsNot Nothing AndAlso GetLabelImpl(descendantNode) <> Label.Ignored Then
                        Yield descendantNode
                    End If
                Next
            End Function
        End Class
    End Class
End Namespace
