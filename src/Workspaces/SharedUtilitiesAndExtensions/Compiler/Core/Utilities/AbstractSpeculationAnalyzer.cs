﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Shared.Utilities
{
    /// <summary>
    /// Helper class to analyze the semantic effects of a speculated syntax node replacement on the parenting nodes.
    /// Given an expression node from a syntax tree and a new expression from a different syntax tree,
    /// it replaces the expression with the new expression to create a speculated syntax tree.
    /// It uses the original tree's semantic model to create a speculative semantic model and verifies that
    /// the syntax replacement doesn't break the semantics of any parenting nodes of the original expression.
    /// </summary>
    internal abstract class AbstractSpeculationAnalyzer<
            TExpressionSyntax,
            TTypeSyntax,
            TAttributeSyntax,
            TArgumentSyntax,
            TForEachStatementSyntax,
            TThrowStatementSyntax,
            TConversion>
        where TExpressionSyntax : SyntaxNode
        where TTypeSyntax : TExpressionSyntax
        where TAttributeSyntax : SyntaxNode
        where TArgumentSyntax : SyntaxNode
        where TForEachStatementSyntax : SyntaxNode
        where TThrowStatementSyntax : SyntaxNode
        where TConversion : struct
    {
        private readonly TExpressionSyntax _expression;
        private readonly TExpressionSyntax _newExpressionForReplace;
        private readonly SemanticModel _semanticModel;
        private readonly CancellationToken _cancellationToken;
        private readonly bool _skipVerificationForReplacedNode;
        private readonly bool _failOnOverloadResolutionFailuresInOriginalCode;
        private readonly bool _isNewSemanticModelSpeculativeModel;

        private SyntaxNode? _lazySemanticRootOfOriginalExpression;
        private TExpressionSyntax? _lazyReplacedExpression;
        private SyntaxNode? _lazySemanticRootOfReplacedExpression;
        private SemanticModel? _lazySpeculativeSemanticModel;

        /// <summary>
        /// Creates a semantic analyzer for speculative syntax replacement.
        /// </summary>
        /// <param name="expression">Original expression to be replaced.</param>
        /// <param name="newExpression">New expression to replace the original expression.</param>
        /// <param name="semanticModel">Semantic model of <paramref name="expression"/> node's syntax tree.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="skipVerificationForReplacedNode">
        /// True if semantic analysis should be skipped for the replaced node and performed starting from parent of the original and replaced nodes.
        /// This could be the case when custom verifications are required to be done by the caller or
        /// semantics of the replaced expression are different from the original expression.
        /// </param>
        /// <param name="failOnOverloadResolutionFailuresInOriginalCode">
        /// True if semantic analysis should fail when any of the invocation expression ancestors of <paramref name="expression"/> in original code has overload resolution failures.
        /// </param>
        public AbstractSpeculationAnalyzer(
            TExpressionSyntax expression,
            TExpressionSyntax newExpression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            bool skipVerificationForReplacedNode = false,
            bool failOnOverloadResolutionFailuresInOriginalCode = false)
        {
            _expression = expression;
            _newExpressionForReplace = newExpression;
            _semanticModel = semanticModel;
            _cancellationToken = cancellationToken;
            _skipVerificationForReplacedNode = skipVerificationForReplacedNode;
            _failOnOverloadResolutionFailuresInOriginalCode = failOnOverloadResolutionFailuresInOriginalCode;
            _isNewSemanticModelSpeculativeModel = true;
            _lazyReplacedExpression = null;
            _lazySemanticRootOfOriginalExpression = null;
            _lazySemanticRootOfReplacedExpression = null;
            _lazySpeculativeSemanticModel = null;
        }

        /// <summary>
        /// Original expression to be replaced.
        /// </summary>
        public TExpressionSyntax OriginalExpression => _expression;

        /// <summary>
        /// First ancestor of <see cref="OriginalExpression"/> which is either a statement, attribute, constructor initializer,
        /// field initializer, default parameter initializer or type syntax node.
        /// It serves as the root node for all semantic analysis for this syntax replacement.
        /// </summary>
        public SyntaxNode SemanticRootOfOriginalExpression
        {
            get
            {
                if (_lazySemanticRootOfOriginalExpression == null)
                {
                    _lazySemanticRootOfOriginalExpression = GetSemanticRootForSpeculation(this.OriginalExpression);
                    RoslynDebug.AssertNotNull(_lazySemanticRootOfOriginalExpression);
                }

                return _lazySemanticRootOfOriginalExpression;
            }
        }

        /// <summary>
        /// Semantic model for the syntax tree corresponding to <see cref="OriginalExpression"/>
        /// </summary>
        public SemanticModel OriginalSemanticModel => _semanticModel;

        /// <summary>
        /// Node which replaces the <see cref="OriginalExpression"/>.
        /// Note that this node is a cloned version of <see cref="_newExpressionForReplace"/> node, which has been re-parented
        /// under the node to be speculated, i.e. <see cref="SemanticRootOfReplacedExpression"/>.
        /// </summary>
        public TExpressionSyntax ReplacedExpression
        {
            get
            {
                EnsureReplacedExpressionAndSemanticRoot();
                return _lazyReplacedExpression;
            }
        }

        /// <summary>
        /// Node created by replacing <see cref="OriginalExpression"/> under <see cref="SemanticRootOfOriginalExpression"/> node.
        /// This node is used as the argument to the GetSpeculativeSemanticModel API and serves as the root node for all
        /// semantic analysis of the speculated tree.
        /// </summary>
        public SyntaxNode SemanticRootOfReplacedExpression
        {
            get
            {
                EnsureReplacedExpressionAndSemanticRoot();
                return _lazySemanticRootOfReplacedExpression;
            }
        }

        /// <summary>
        /// Speculative semantic model used for analyzing the semantics of the new tree.
        /// </summary>
        public SemanticModel SpeculativeSemanticModel
        {
            get
            {
                EnsureSpeculativeSemanticModel();
                return _lazySpeculativeSemanticModel;
            }
        }

        public CancellationToken CancellationToken => _cancellationToken;

        protected abstract SyntaxNode GetSemanticRootForSpeculation(TExpressionSyntax expression);

        protected virtual SyntaxNode GetSemanticRootOfReplacedExpression(SyntaxNode semanticRootOfOriginalExpression, TExpressionSyntax annotatedReplacedExpression)
            => semanticRootOfOriginalExpression.ReplaceNode(this.OriginalExpression, annotatedReplacedExpression);

        [MemberNotNull(nameof(_lazySemanticRootOfReplacedExpression), nameof(_lazyReplacedExpression))]
        private void EnsureReplacedExpressionAndSemanticRoot()
        {
            if (_lazySemanticRootOfReplacedExpression == null)
            {
                // Because the new expression will change identity once we replace the old
                // expression in its parent, we annotate it here to allow us to get back to
                // it after replace.
                var annotation = new SyntaxAnnotation();
                var annotatedExpression = _newExpressionForReplace.WithAdditionalAnnotations(annotation);
                _lazySemanticRootOfReplacedExpression = GetSemanticRootOfReplacedExpression(this.SemanticRootOfOriginalExpression, annotatedExpression);
                _lazyReplacedExpression = (TExpressionSyntax)_lazySemanticRootOfReplacedExpression.GetAnnotatedNodesAndTokens(annotation).Single().AsNode()!;
            }
            else
            {
                RoslynDebug.AssertNotNull(_lazyReplacedExpression);
            }
        }

        [Conditional("DEBUG")]
        protected abstract void ValidateSpeculativeSemanticModel(SemanticModel speculativeSemanticModel, SyntaxNode nodeToSpeculate);

        [MemberNotNull(nameof(_lazySpeculativeSemanticModel))]
        private void EnsureSpeculativeSemanticModel()
        {
            if (_lazySpeculativeSemanticModel == null)
            {
                var nodeToSpeculate = this.SemanticRootOfReplacedExpression;
                _lazySpeculativeSemanticModel = CreateSpeculativeSemanticModel(this.SemanticRootOfOriginalExpression, nodeToSpeculate, _semanticModel);
                ValidateSpeculativeSemanticModel(_lazySpeculativeSemanticModel, nodeToSpeculate);
            }
        }

        protected abstract SemanticModel CreateSpeculativeSemanticModel(SyntaxNode originalNode, SyntaxNode nodeToSpeculate, SemanticModel semanticModel);

        #region Semantic comparison helpers

        protected virtual bool ReplacementIntroducesErrorType(TExpressionSyntax originalExpression, TExpressionSyntax newExpression)
        {
            RoslynDebug.AssertNotNull(originalExpression);
            Debug.Assert(this.SemanticRootOfOriginalExpression.DescendantNodesAndSelf().Contains(originalExpression));
            RoslynDebug.AssertNotNull(newExpression);
            Debug.Assert(this.SemanticRootOfReplacedExpression.DescendantNodesAndSelf().Contains(newExpression));

            var originalTypeInfo = this.OriginalSemanticModel.GetTypeInfo(originalExpression);
            var newTypeInfo = this.SpeculativeSemanticModel.GetTypeInfo(newExpression);
            if (originalTypeInfo.Type == null)
            {
                return false;
            }

            return newTypeInfo.Type == null ||
                (newTypeInfo.Type.IsErrorType() && !originalTypeInfo.Type.IsErrorType());
        }

        protected bool TypesAreCompatible(TExpressionSyntax originalExpression, TExpressionSyntax newExpression)
        {
            RoslynDebug.AssertNotNull(originalExpression);
            Debug.Assert(this.SemanticRootOfOriginalExpression.DescendantNodesAndSelf().Contains(originalExpression));
            RoslynDebug.AssertNotNull(newExpression);
            Debug.Assert(this.SemanticRootOfReplacedExpression.DescendantNodesAndSelf().Contains(newExpression));

            var originalTypeInfo = this.OriginalSemanticModel.GetTypeInfo(originalExpression);
            var newTypeInfo = this.SpeculativeSemanticModel.GetTypeInfo(newExpression);
            return SymbolsAreCompatible(originalTypeInfo.Type, newTypeInfo.Type);
        }

        protected bool ConvertedTypesAreCompatible(TExpressionSyntax originalExpression, TExpressionSyntax newExpression)
        {
            RoslynDebug.AssertNotNull(originalExpression);
            Debug.Assert(this.SemanticRootOfOriginalExpression.DescendantNodesAndSelf().Contains(originalExpression));
            RoslynDebug.AssertNotNull(newExpression);
            Debug.Assert(this.SemanticRootOfReplacedExpression.DescendantNodesAndSelf().Contains(newExpression));

            var originalTypeInfo = this.OriginalSemanticModel.GetTypeInfo(originalExpression);
            var newTypeInfo = this.SpeculativeSemanticModel.GetTypeInfo(newExpression);
            return SymbolsAreCompatible(originalTypeInfo.ConvertedType, newTypeInfo.ConvertedType);
        }

        protected bool ImplicitConversionsAreCompatible(TExpressionSyntax originalExpression, TExpressionSyntax newExpression)
        {
            RoslynDebug.AssertNotNull(originalExpression);
            Debug.Assert(this.SemanticRootOfOriginalExpression.DescendantNodesAndSelf().Contains(originalExpression));
            RoslynDebug.AssertNotNull(newExpression);
            Debug.Assert(this.SemanticRootOfReplacedExpression.DescendantNodesAndSelf().Contains(newExpression));

            return ConversionsAreCompatible(this.OriginalSemanticModel, originalExpression, this.SpeculativeSemanticModel, newExpression);
        }

        private bool ImplicitConversionsAreCompatible(TExpressionSyntax originalExpression, ITypeSymbol originalTargetType, TExpressionSyntax newExpression, ITypeSymbol newTargetType)
        {
            RoslynDebug.AssertNotNull(originalExpression);
            Debug.Assert(this.SemanticRootOfOriginalExpression.DescendantNodesAndSelf().Contains(originalExpression));
            RoslynDebug.AssertNotNull(newExpression);
            Debug.Assert(this.SemanticRootOfReplacedExpression.DescendantNodesAndSelf().Contains(newExpression));
            RoslynDebug.AssertNotNull(originalTargetType);
            RoslynDebug.AssertNotNull(newTargetType);

            return ConversionsAreCompatible(originalExpression, originalTargetType, newExpression, newTargetType);
        }

        protected abstract bool ConversionsAreCompatible(SemanticModel model1, TExpressionSyntax expression1, SemanticModel model2, TExpressionSyntax expression2);
        protected abstract bool ConversionsAreCompatible(TExpressionSyntax originalExpression, ITypeSymbol originalTargetType, TExpressionSyntax newExpression, ITypeSymbol newTargetType);

        protected bool SymbolsAreCompatible(SyntaxNode originalNode, SyntaxNode newNode, bool requireNonNullSymbols = false)
        {
            RoslynDebug.AssertNotNull(originalNode);
            Debug.Assert(this.SemanticRootOfOriginalExpression.DescendantNodesAndSelf().Contains(originalNode));
            RoslynDebug.AssertNotNull(newNode);
            Debug.Assert(this.SemanticRootOfReplacedExpression.DescendantNodesAndSelf().Contains(newNode));

            var originalSymbolInfo = this.OriginalSemanticModel.GetSymbolInfo(originalNode);
            var newSymbolInfo = this.SpeculativeSemanticModel.GetSymbolInfo(newNode);
            return SymbolInfosAreCompatible(originalSymbolInfo, newSymbolInfo, requireNonNullSymbols);
        }

        public static bool SymbolInfosAreCompatible(SymbolInfo originalSymbolInfo, SymbolInfo newSymbolInfo, bool performEquivalenceCheck, bool requireNonNullSymbols = false)
        {
            return originalSymbolInfo.CandidateReason == newSymbolInfo.CandidateReason &&
                SymbolsAreCompatibleCore(originalSymbolInfo.Symbol, newSymbolInfo.Symbol, performEquivalenceCheck, requireNonNullSymbols);
        }

        protected bool SymbolInfosAreCompatible(SymbolInfo originalSymbolInfo, SymbolInfo newSymbolInfo, bool requireNonNullSymbols = false)
            => SymbolInfosAreCompatible(originalSymbolInfo, newSymbolInfo, performEquivalenceCheck: !_isNewSemanticModelSpeculativeModel, requireNonNullSymbols: requireNonNullSymbols);

        protected bool SymbolsAreCompatible(ISymbol? symbol, ISymbol? newSymbol, bool requireNonNullSymbols = false)
            => SymbolsAreCompatibleCore(symbol, newSymbol, performEquivalenceCheck: !_isNewSemanticModelSpeculativeModel, requireNonNullSymbols: requireNonNullSymbols);

        private static bool SymbolsAreCompatibleCore(
            ISymbol? symbol,
            ISymbol? newSymbol,
            bool performEquivalenceCheck,
            bool requireNonNullSymbols = false)
        {
            if (symbol == null && newSymbol == null)
            {
                return !requireNonNullSymbols;
            }

            if (symbol == null || newSymbol == null)
            {
                return false;
            }

            if (symbol.IsReducedExtension())
            {
                symbol = ((IMethodSymbol)symbol).GetConstructedReducedFrom()!;
            }

            if (newSymbol.IsReducedExtension())
            {
                newSymbol = ((IMethodSymbol)newSymbol).GetConstructedReducedFrom()!;
            }

            // TODO: Lambda function comparison performs syntax equality, hence is non-trivial to compare lambda methods across different compilations.
            // For now, just assume they are equal.
            if (symbol.IsAnonymousFunction())
            {
                return newSymbol.IsAnonymousFunction();
            }

            if (performEquivalenceCheck)
            {
                // We are comparing symbols across two semantic models (where neither is the speculative model of other one).
                // We will use the SymbolEquivalenceComparer to check if symbols are equivalent.
                return CompareAcrossSemanticModels(symbol, newSymbol);
            }

            if (symbol.Equals(newSymbol, SymbolEqualityComparer.IncludeNullability))
            {
                return true;
            }

            if (symbol is IMethodSymbol methodSymbol && newSymbol is IMethodSymbol newMethodSymbol)
            {
                // If we have local functions, we can't use normal symbol equality for them (since that checks locations).
                // Have to defer to SymbolEquivalence instead.
                if (methodSymbol.MethodKind == MethodKind.LocalFunction && newMethodSymbol.MethodKind == MethodKind.LocalFunction)
                    return CompareAcrossSemanticModels(methodSymbol, newMethodSymbol);

                // Handle equivalence of special built-in comparison operators between enum types and 
                // it's underlying enum type.
                if (methodSymbol.TryGetPredefinedComparisonOperator(out var originalOp) &&
                    newMethodSymbol.TryGetPredefinedComparisonOperator(out var newOp) &&
                    originalOp == newOp)
                {
                    var type = methodSymbol.ContainingType;
                    var newType = newMethodSymbol.ContainingType;
                    if (type != null && newType != null)
                    {
                        if (EnumTypesAreCompatible(type, newType) ||
                            EnumTypesAreCompatible(newType, type))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool CompareAcrossSemanticModels(ISymbol symbol, ISymbol newSymbol)
        {
            // SymbolEquivalenceComparer performs Location equality checks for locals, labels, range-variables and local
            // functions. As we are comparing symbols from different semantic models, locations will differ. Hence
            // perform minimal checks for these symbol kinds.

            if (symbol.Kind != newSymbol.Kind)
                return false;

            if (symbol is ILocalSymbol localSymbol && newSymbol is ILocalSymbol newLocalSymbol)
            {
                return newSymbol.IsImplicitlyDeclared == symbol.IsImplicitlyDeclared &&
                       symbol.Name == newSymbol.Name &&
                       CompareAcrossSemanticModels(localSymbol.Type, newLocalSymbol.Type);
            }

            if (symbol is ILabelSymbol && newSymbol is ILabelSymbol)
                return symbol.Name == newSymbol.Name;

            if (symbol is IRangeVariableSymbol && newSymbol is IRangeVariableSymbol)
                return symbol.Name == newSymbol.Name;

            if (symbol is IParameterSymbol parameterSymbol &&
                newSymbol is IParameterSymbol newParameterSymbol &&
                parameterSymbol.ContainingSymbol.IsAnonymousOrLocalFunction() &&
                newParameterSymbol.ContainingSymbol.IsAnonymousOrLocalFunction())
            {
                return symbol.Name == newSymbol.Name &&
                       parameterSymbol.IsRefOrOut() == newParameterSymbol.IsRefOrOut() &&
                       CompareAcrossSemanticModels(parameterSymbol.Type, newParameterSymbol.Type);
            }

            if (symbol is IMethodSymbol methodSymbol &&
                newSymbol is IMethodSymbol newMethodSymbol &&
                methodSymbol.IsLocalFunction() &&
                newMethodSymbol.IsLocalFunction())
            {
                return symbol.Name == newSymbol.Name &&
                       methodSymbol.Parameters.Length == newMethodSymbol.Parameters.Length &&
                       CompareAcrossSemanticModels(methodSymbol.ReturnType, newMethodSymbol.ReturnType) &&
                       methodSymbol.Parameters.Zip(newMethodSymbol.Parameters, (p1, p2) => (p1, p2)).All(
                           t => CompareAcrossSemanticModels(t.p1, t.p2));
            }

            return SymbolEquivalenceComparer.Instance.Equals(symbol, newSymbol);
        }

        private static bool EnumTypesAreCompatible(INamedTypeSymbol type1, INamedTypeSymbol type2)
            => type1.IsEnumType() &&
               type1.EnumUnderlyingType?.SpecialType == type2.SpecialType;

        #endregion

        /// <summary>
        /// Determines whether performing the given syntax replacement will change the semantics of any parenting expressions
        /// by performing a bottom up walk from the <see cref="OriginalExpression"/> up to <see cref="SemanticRootOfOriginalExpression"/>
        /// in the original tree and simultaneously walking bottom up from <see cref="ReplacedExpression"/> up to <see cref="SemanticRootOfReplacedExpression"/>
        /// in the speculated syntax tree and performing appropriate semantic comparisons.
        /// </summary>
        public bool ReplacementChangesSemantics()
        {
            if (this.SemanticRootOfOriginalExpression is TTypeSyntax)
            {
                var originalType = (TTypeSyntax)this.OriginalExpression;
                var newType = (TTypeSyntax)this.ReplacedExpression;
                return ReplacementBreaksTypeResolution(originalType, newType, useSpeculativeModel: false);
            }

            return ReplacementChangesSemantics(
                currentOriginalNode: this.OriginalExpression,
                currentReplacedNode: this.ReplacedExpression,
                originalRoot: this.SemanticRootOfOriginalExpression,
                skipVerificationForCurrentNode: _skipVerificationForReplacedNode);
        }

        protected abstract bool IsParenthesizedExpression([NotNullWhen(true)] SyntaxNode? node);

        protected bool ReplacementChangesSemantics(SyntaxNode currentOriginalNode, SyntaxNode currentReplacedNode, SyntaxNode originalRoot, bool skipVerificationForCurrentNode)
        {
            if (this.SpeculativeSemanticModel == null)
            {
                // This is possible for some broken code scenarios with parse errors, bail out gracefully here.
                return true;
            }

            SyntaxNode? previousOriginalNode = null, previousReplacedNode = null;

            while (true)
            {
                if (!skipVerificationForCurrentNode &&
                    ReplacementChangesSemanticsForNode(
                        currentOriginalNode, currentReplacedNode,
                        previousOriginalNode, previousReplacedNode))
                {
                    return true;
                }

                if (currentOriginalNode == originalRoot)
                {
                    break;
                }

                RoslynDebug.AssertNotNull(currentOriginalNode.Parent);
                RoslynDebug.AssertNotNull(currentReplacedNode.Parent);

                previousOriginalNode = currentOriginalNode;
                previousReplacedNode = currentReplacedNode;
                currentOriginalNode = currentOriginalNode.Parent;
                currentReplacedNode = currentReplacedNode.Parent;
                skipVerificationForCurrentNode = skipVerificationForCurrentNode && IsParenthesizedExpression(currentReplacedNode);
            }

            return false;
        }

        /// <summary>
        /// Checks whether the semantic symbols for the <see cref="OriginalExpression"/> and <see cref="ReplacedExpression"/> are non-null and compatible.
        /// </summary>
        /// <returns></returns>
        public bool SymbolsForOriginalAndReplacedNodesAreCompatible()
        {
            if (this.SpeculativeSemanticModel == null)
            {
                // This is possible for some broken code scenarios with parse errors, bail out gracefully here.
                return false;
            }

            return SymbolsAreCompatible(this.OriginalExpression, this.ReplacedExpression, requireNonNullSymbols: true);
        }

        protected abstract bool ReplacementChangesSemanticsForNodeLanguageSpecific(SyntaxNode currentOriginalNode, SyntaxNode currentReplacedNode, SyntaxNode? previousOriginalNode, SyntaxNode? previousReplacedNode);

        private bool ReplacementChangesSemanticsForNode(SyntaxNode currentOriginalNode, SyntaxNode currentReplacedNode, SyntaxNode? previousOriginalNode, SyntaxNode? previousReplacedNode)
        {
            Debug.Assert(previousOriginalNode == null || previousOriginalNode.Parent == currentOriginalNode);
            Debug.Assert(previousReplacedNode == null || previousReplacedNode.Parent == currentReplacedNode);

            if (!OperationsAreCompatible(currentOriginalNode, currentReplacedNode))
                return true;

            if (ExpressionMightReferenceMember(currentOriginalNode))
            {
                // If replacing the node will result in a change in overload resolution, we won't remove it.
                var originalExpression = (TExpressionSyntax)currentOriginalNode;
                var newExpression = (TExpressionSyntax)currentReplacedNode;
                if (ReplacementBreaksExpression(originalExpression, newExpression))
                {
                    return true;
                }

                if (ReplacementBreaksSystemObjectMethodResolution(currentOriginalNode, currentReplacedNode, previousOriginalNode, previousReplacedNode))
                {
                    return true;
                }

                return !ImplicitConversionsAreCompatible(originalExpression, newExpression);
            }
            else if (currentOriginalNode is TForEachStatementSyntax originalForEachStatement)
            {
                var newForEachStatement = (TForEachStatementSyntax)currentReplacedNode;
                return ReplacementBreaksForEachStatement(originalForEachStatement, newForEachStatement);
            }
            else if (currentOriginalNode is TAttributeSyntax originalAttribute)
            {
                var newAttribute = (TAttributeSyntax)currentReplacedNode;
                return ReplacementBreaksAttribute(originalAttribute, newAttribute);
            }
            else if (currentOriginalNode is TThrowStatementSyntax originalThrowStatement)
            {
                var newThrowStatement = (TThrowStatementSyntax)currentReplacedNode;
                return ReplacementBreaksThrowStatement(originalThrowStatement, newThrowStatement);
            }
            else if (ReplacementChangesSemanticsForNodeLanguageSpecific(currentOriginalNode, currentReplacedNode, previousOriginalNode, previousReplacedNode))
            {
                return true;
            }

            if (currentOriginalNode is TTypeSyntax originalType)
            {
                var newType = (TTypeSyntax)currentReplacedNode;
                return ReplacementBreaksTypeResolution(originalType, newType);
            }
            else if (currentOriginalNode is TExpressionSyntax originalExpression)
            {
                var newExpression = (TExpressionSyntax)currentReplacedNode;
                if (!ImplicitConversionsAreCompatible(originalExpression, newExpression) ||
                    ReplacementIntroducesErrorType(originalExpression, newExpression))
                {
                    return true;
                }
            }

            return false;
        }

        private bool OperationsAreCompatible(SyntaxNode currentOriginalNode, SyntaxNode currentReplacedNode)
        {
            var originalOperation = this._semanticModel.GetOperation(currentOriginalNode, CancellationToken);
            var currentOperation = this.SpeculativeSemanticModel.GetOperation(currentReplacedNode, CancellationToken);

            if (originalOperation is IInvocationOperation originalInvocation)
            {
                // Invocations must stay invocations after update.
                if (currentOperation is not IInvocationOperation currentInvocation)
                    return false;

                // An instance call must stay an instance call (and a static call must stay a static call).
                if (IsNullOrNone(originalInvocation.Instance) != IsNullOrNone(currentInvocation.Instance))
                    return false;

                // Add more invocation tests here.
            }

            // Add more operation tests here.
            return true;
        }

        private static bool IsNullOrNone(IOperation? instance)
            => instance is null || instance.Kind == OperationKind.None;

        /// <summary>
        /// Determine if removing the cast could cause the semantics of System.Object method call to change.
        /// E.g. Dim b = CStr(1).GetType() is necessary, but the GetType method symbol info resolves to the same with or without the cast.
        /// </summary>
        private bool ReplacementBreaksSystemObjectMethodResolution(SyntaxNode currentOriginalNode, SyntaxNode currentReplacedNode, [NotNullWhen(true)] SyntaxNode? previousOriginalNode, [NotNullWhen(true)] SyntaxNode? previousReplacedNode)
        {
            if (previousOriginalNode != null && previousReplacedNode != null)
            {
                var originalExpressionSymbol = this.OriginalSemanticModel.GetSymbolInfo(currentOriginalNode).Symbol;
                var replacedExpressionSymbol = this.SpeculativeSemanticModel.GetSymbolInfo(currentReplacedNode).Symbol;

                if (IsSymbolSystemObjectInstanceMethod(originalExpressionSymbol) && IsSymbolSystemObjectInstanceMethod(replacedExpressionSymbol))
                {
                    var previousOriginalType = this.OriginalSemanticModel.GetTypeInfo(previousOriginalNode).Type;
                    var previousReplacedType = this.SpeculativeSemanticModel.GetTypeInfo(previousReplacedNode).Type;
                    if (previousReplacedType != null && previousOriginalType != null)
                    {
                        return !previousReplacedType.InheritsFromOrEquals(previousOriginalType);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if the symbol is a non-overridable, non static method on System.Object (e.g. GetType)
        /// </summary>
        private static bool IsSymbolSystemObjectInstanceMethod([NotNullWhen(true)] ISymbol? symbol)
        {
            return symbol != null
                && symbol.IsKind(SymbolKind.Method)
                && symbol.ContainingType?.SpecialType == SpecialType.System_Object
                && !symbol.IsOverridable()
                && !symbol.IsStaticType();
        }

        private bool ReplacementBreaksAttribute(TAttributeSyntax attribute, TAttributeSyntax newAttribute)
        {
            var attributeSym = this.OriginalSemanticModel.GetSymbolInfo(attribute).Symbol;
            var newAttributeSym = this.SpeculativeSemanticModel.GetSymbolInfo(newAttribute).Symbol;
            return !SymbolsAreCompatible(attributeSym, newAttributeSym);
        }

        protected abstract TExpressionSyntax GetForEachStatementExpression(TForEachStatementSyntax forEachStatement);

        protected abstract bool IsForEachTypeInferred(TForEachStatementSyntax forEachStatement, SemanticModel semanticModel);

        private bool ReplacementBreaksForEachStatement(TForEachStatementSyntax forEachStatement, TForEachStatementSyntax newForEachStatement)
        {
            var forEachExpression = GetForEachStatementExpression(forEachStatement);
            if (forEachExpression.IsMissing ||
                !forEachExpression.Span.Contains(_expression.SpanStart))
            {
                return false;
            }

            // inferred variable type compatible
            if (IsForEachTypeInferred(forEachStatement, _semanticModel))
            {
                var local = (ILocalSymbol)_semanticModel.GetRequiredDeclaredSymbol(forEachStatement, _cancellationToken);
                var newLocal = (ILocalSymbol)this.SpeculativeSemanticModel.GetRequiredDeclaredSymbol(newForEachStatement, _cancellationToken);
                if (!SymbolsAreCompatible(local.Type, newLocal.Type))
                {
                    return true;
                }
            }

            GetForEachSymbols(this.OriginalSemanticModel, forEachStatement, out var originalGetEnumerator, out var originalElementType);
            GetForEachSymbols(this.SpeculativeSemanticModel, newForEachStatement, out var newGetEnumerator, out var newElementType);

            var newForEachExpression = GetForEachStatementExpression(newForEachStatement);

            if (ReplacementBreaksForEachGetEnumerator(originalGetEnumerator, newGetEnumerator, newForEachExpression) ||
                !ForEachConversionsAreCompatible(this.OriginalSemanticModel, forEachStatement, this.SpeculativeSemanticModel, newForEachStatement) ||
                !SymbolsAreCompatible(originalElementType, newElementType))
            {
                return true;
            }

            return false;
        }

        protected abstract bool ForEachConversionsAreCompatible(SemanticModel originalModel, TForEachStatementSyntax originalForEach, SemanticModel newModel, TForEachStatementSyntax newForEach);

        protected abstract void GetForEachSymbols(SemanticModel model, TForEachStatementSyntax forEach, out IMethodSymbol getEnumeratorMethod, out ITypeSymbol elementType);

        private bool ReplacementBreaksForEachGetEnumerator(IMethodSymbol getEnumerator, IMethodSymbol newGetEnumerator, TExpressionSyntax newForEachStatementExpression)
        {
            if (getEnumerator == null && newGetEnumerator == null)
            {
                return false;
            }

            if (getEnumerator == null || newGetEnumerator == null)
            {
                return true;
            }

            if (getEnumerator.ToSignatureDisplayString() != newGetEnumerator.ToSignatureDisplayString())
            {
                // Note this is likely an interface member from IEnumerable but the new member may be a
                // GetEnumerator method on a specific type.
                if (getEnumerator.IsImplementableMember())
                {
                    var expressionType = this.SpeculativeSemanticModel.GetTypeInfo(newForEachStatementExpression, _cancellationToken).ConvertedType;
                    if (expressionType != null)
                    {
                        var implementationMember = expressionType.FindImplementationForInterfaceMember(getEnumerator);
                        if (implementationMember != null)
                        {
                            if (implementationMember.ToSignatureDisplayString() != newGetEnumerator.ToSignatureDisplayString())
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        protected abstract TExpressionSyntax GetThrowStatementExpression(TThrowStatementSyntax throwStatement);

        private bool ReplacementBreaksThrowStatement(TThrowStatementSyntax originalThrowStatement, TThrowStatementSyntax newThrowStatement)
        {
            var originalThrowExpression = GetThrowStatementExpression(originalThrowStatement);
            var originalThrowExpressionType = this.OriginalSemanticModel.GetTypeInfo(originalThrowExpression).Type;
            var newThrowExpression = GetThrowStatementExpression(newThrowStatement);
            var newThrowExpressionType = this.SpeculativeSemanticModel.GetTypeInfo(newThrowExpression).Type;

            // C# language specification requires that type of the expression passed to ThrowStatement is or derives from System.Exception.
            return originalThrowExpressionType.IsOrDerivesFromExceptionType(this.OriginalSemanticModel.Compilation) !=
                newThrowExpressionType.IsOrDerivesFromExceptionType(this.SpeculativeSemanticModel.Compilation);
        }

        protected abstract bool IsInNamespaceOrTypeContext(TExpressionSyntax node);

        private bool ReplacementBreaksTypeResolution(TTypeSyntax type, TTypeSyntax newType, bool useSpeculativeModel = true)
        {
            var symbol = this.OriginalSemanticModel.GetSymbolInfo(type).Symbol;

            ISymbol? newSymbol;
            if (useSpeculativeModel)
            {
                newSymbol = this.SpeculativeSemanticModel.GetSymbolInfo(newType, _cancellationToken).Symbol;
            }
            else
            {
                var bindingOption = IsInNamespaceOrTypeContext(type) ? SpeculativeBindingOption.BindAsTypeOrNamespace : SpeculativeBindingOption.BindAsExpression;
                newSymbol = this.OriginalSemanticModel.GetSpeculativeSymbolInfo(type.SpanStart, newType, bindingOption).Symbol;
            }

            return symbol != null && !SymbolsAreCompatible(symbol, newSymbol);
        }

        protected abstract bool ExpressionMightReferenceMember([NotNullWhen(true)] SyntaxNode? node);

        private static bool IsDelegateInvoke(ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Method &&
                ((IMethodSymbol)symbol).MethodKind == MethodKind.DelegateInvoke;
        }

        private static bool IsAnonymousDelegateInvoke(ISymbol symbol)
        {
            return IsDelegateInvoke(symbol) &&
                symbol.ContainingType != null &&
                symbol.ContainingType.IsAnonymousType();
        }

        private bool ReplacementBreaksExpression(TExpressionSyntax expression, TExpressionSyntax newExpression)
        {
            var originalSymbolInfo = _semanticModel.GetSymbolInfo(expression);
            if (_failOnOverloadResolutionFailuresInOriginalCode && originalSymbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure)
            {
                return true;
            }

            var newSymbolInfo = this.SpeculativeSemanticModel.GetSymbolInfo(node: newExpression);
            var symbol = originalSymbolInfo.Symbol;
            var newSymbol = newSymbolInfo.Symbol;

            if (SymbolInfosAreCompatible(originalSymbolInfo, newSymbolInfo))
            {
                // Original and new symbols for the invocation expression are compatible.
                // However, if the symbols are interface members and if the receiver symbol for one of the expressions is a possible ValueType type parameter,
                // and the other one is not, then there might be a boxing conversion at runtime which causes different runtime behavior.
                if (symbol.IsImplementableMember())
                {
                    if (IsReceiverNonUniquePossibleValueTypeParam(expression, this.OriginalSemanticModel) !=
                        IsReceiverNonUniquePossibleValueTypeParam(newExpression, this.SpeculativeSemanticModel))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (symbol == null || newSymbol == null || originalSymbolInfo.CandidateReason != newSymbolInfo.CandidateReason)
            {
                return true;
            }

            if (newSymbol.IsOverride)
            {
                for (var overriddenMember = newSymbol.GetOverriddenMember(); overriddenMember != null; overriddenMember = overriddenMember.GetOverriddenMember())
                {
                    if (symbol.Equals(overriddenMember))
                        return !SymbolsHaveCompatibleParameterLists(symbol, newSymbol, expression);
                }
            }

            if (symbol.IsImplementableMember() &&
                IsCompatibleInterfaceMemberImplementation(
                    symbol, newSymbol, expression, newExpression, this.SpeculativeSemanticModel))
            {
                return false;
            }

            // Allow speculated invocation expression to bind to a different method symbol if the method's containing type is a delegate type
            // which has a delegate variance conversion to/from the original method's containing delegate type.
            if (newSymbol.ContainingType.IsDelegateType() &&
                symbol.ContainingType.IsDelegateType() &&
                IsReferenceConversion(this.OriginalSemanticModel.Compilation, newSymbol.ContainingType, symbol.ContainingType))
            {
                return false;
            }

            // Heuristic: If we now bind to an anonymous delegate's invoke method, assume that
            // this isn't a change in overload resolution.
            if (IsAnonymousDelegateInvoke(newSymbol))
            {
                return false;
            }

            return true;
        }

        protected bool ReplacementBreaksCompoundAssignment(
            TExpressionSyntax originalLeft,
            TExpressionSyntax originalRight,
            TExpressionSyntax newLeft,
            TExpressionSyntax newRight)
        {
            var originalTargetType = this.OriginalSemanticModel.GetTypeInfo(originalLeft).Type;
            if (originalTargetType != null)
            {
                var newTargetType = this.SpeculativeSemanticModel.GetTypeInfo(newLeft).Type;
                return !SymbolsAreCompatible(originalTargetType, newTargetType) ||
                    !ImplicitConversionsAreCompatible(originalRight, originalTargetType, newRight, newTargetType!);
            }

            return false;
        }

        protected abstract bool IsReferenceConversion(Compilation model, ITypeSymbol sourceType, ITypeSymbol targetType);

        private bool IsCompatibleInterfaceMemberImplementation(
            ISymbol symbol,
            ISymbol newSymbol,
            TExpressionSyntax originalExpression,
            TExpressionSyntax newExpression,
            SemanticModel speculativeSemanticModel)
        {
            // In general, we don't want to remove casts to interfaces.  It may have subtle changes in behavior,
            // especially if the types in question change in the future.  For example, if a type becomes non-sealed or a
            // new interface impl is introduced, we may subtly break things.
            //
            // The only cases where we feel confident enough to elide the cast are:
            //
            // 1. When we have an Array/Delegate/Enum. These are such core types, and cannot be changed by teh user,
            //    that we can trust their impls to not change.
            // 2. We have one of the builtin structs (like int). These are such core types, and cannot be changed by teh
            //    user, that we can trust their impls to not change.
            // 3. if we have a struct and we know we have a fresh copy of it.  In that case, boxing the struct to the
            //    interface doesn't serve any purpose.

            var newSymbolContainingType = newSymbol.ContainingType;
            if (newSymbolContainingType == null)
                return false;

            var newReceiver = GetReceiver(newExpression);
            var newReceiverType = newReceiver != null
                ? speculativeSemanticModel.GetTypeInfo(newReceiver).ConvertedType
                : newSymbolContainingType;

            if (newReceiverType == null)
                return false;

            var implementationMember = newSymbolContainingType.FindImplementationForInterfaceMember(symbol);
            if (implementationMember == null)
                return false;

            if (!newSymbol.Equals(implementationMember))
                return false;

            if (!SymbolsHaveCompatibleParameterLists(symbol, implementationMember, originalExpression))
                return false;

            if (newReceiverType.IsValueType)
            {
                // Presume builtin value types are all immutable, and thus will have the same semantics when you call
                // interface members on them directly instead of through a boxed copy.
                if (newReceiverType.SpecialType != SpecialType.None)
                    return true;

                // For non-builtins, only remove the boxing if we know we have a copy already.
                return newReceiver != null && IsReceiverUniqueInstance(newReceiver, speculativeSemanticModel);
            }

            return newSymbolContainingType.SpecialType is SpecialType.System_Array or
                   SpecialType.System_Delegate or
                   SpecialType.System_Enum or
                   SpecialType.System_String;
        }

        private bool IsReceiverNonUniquePossibleValueTypeParam(TExpressionSyntax invocation, SemanticModel semanticModel)
        {
            var receiver = GetReceiver(invocation);
            if (receiver != null)
            {
                var receiverType = semanticModel.GetTypeInfo(receiver).Type;
                if (receiverType.IsKind(SymbolKind.TypeParameter) && !receiverType.IsReferenceType)
                {
                    return !IsReceiverUniqueInstance(receiver, semanticModel);
                }
            }

            return false;
        }

        // Returns true if the given receiver expression for an invocation represents a unique copy of the underlying
        // object that is not referenced by any other variable. For example, if the receiver expression is produced by a
        // method call, property, or indexer, then it will be a fresh receiver in the case of value types.
        private static bool IsReceiverUniqueInstance(TExpressionSyntax receiver, SemanticModel semanticModel)
        {
            var receiverSymbol = semanticModel.GetSymbolInfo(receiver).GetAnySymbol();

            if (receiverSymbol == null)
                return false;

            return receiverSymbol.IsKind(SymbolKind.Method) ||
                   receiverSymbol.IsIndexer() ||
                   receiverSymbol.IsKind(SymbolKind.Property);
        }

        protected abstract ImmutableArray<TArgumentSyntax> GetArguments(TExpressionSyntax expression);
        protected abstract TExpressionSyntax GetReceiver(TExpressionSyntax expression);

        private bool SymbolsHaveCompatibleParameterLists(ISymbol originalSymbol, ISymbol newSymbol, TExpressionSyntax originalInvocation)
        {
            if (originalSymbol.IsKind(SymbolKind.Method) || originalSymbol.IsIndexer())
            {
                var specifiedArguments = GetArguments(originalInvocation);
                if (!specifiedArguments.IsDefault)
                {
                    var symbolParameters = originalSymbol.GetParameters();
                    var newSymbolParameters = newSymbol.GetParameters();
                    return AreCompatibleParameterLists(specifiedArguments, symbolParameters, newSymbolParameters);
                }
            }

            return true;
        }

        protected abstract bool IsNamedArgument(TArgumentSyntax argument);
        protected abstract string GetNamedArgumentIdentifierValueText(TArgumentSyntax argument);

        private bool AreCompatibleParameterLists(
            ImmutableArray<TArgumentSyntax> specifiedArguments,
            ImmutableArray<IParameterSymbol> signature1Parameters,
            ImmutableArray<IParameterSymbol> signature2Parameters)
        {
            Debug.Assert(signature1Parameters.Length == signature2Parameters.Length);
            Debug.Assert(specifiedArguments.Length <= signature1Parameters.Length ||
                        (signature1Parameters.Length > 0 && !signature1Parameters.Last().IsParams));

            if (signature1Parameters.Length != signature2Parameters.Length)
            {
                return false;
            }

            // If there aren't any parameters, we're OK.
            if (signature1Parameters.Length == 0)
            {
                return true;
            }

            // To ensure that the second parameter list is called in the same way as the
            // first, we need to use the specified arguments to bail out if...
            //
            //     * A named argument doesn't have a corresponding parameter in the
            //       in either parameter list, or...
            //
            //     * A named argument matches a parameter that is in a different position
            //       in the two parameter lists.
            //
            // After checking the specified arguments, we walk the unspecified parameters
            // in both parameter lists to ensure that they have matching default values.

            var specifiedParameters1 = new List<IParameterSymbol>();
            var specifiedParameters2 = new List<IParameterSymbol>();

            for (var i = 0; i < specifiedArguments.Length; i++)
            {
                var argument = specifiedArguments[i];

                // Handle named argument
                if (IsNamedArgument(argument))
                {
                    var name = GetNamedArgumentIdentifierValueText(argument);

                    var parameter1 = signature1Parameters.FirstOrDefault(p => p.Name == name);
                    RoslynDebug.AssertNotNull(parameter1);

                    var parameter2 = signature2Parameters.FirstOrDefault(p => p.Name == name);
                    if (parameter2 == null)
                    {
                        return false;
                    }

                    if (signature1Parameters.IndexOf(parameter1) != signature2Parameters.IndexOf(parameter2))
                    {
                        return false;
                    }

                    specifiedParameters1.Add(parameter1);
                    specifiedParameters2.Add(parameter2);
                }
                else
                {
                    // otherwise, treat the argument positionally, taking care to properly
                    // handle params parameters.
                    if (i < signature1Parameters.Length)
                    {
                        specifiedParameters1.Add(signature1Parameters[i]);
                        specifiedParameters2.Add(signature2Parameters[i]);
                    }
                }
            }

            // At this point, we can safely assume that specifiedParameters1 and signature2Parameters
            // contain parameters that appear at the same positions in their respective signatures
            // because we bailed out if named arguments referred to parameters at different positions.
            //
            // Now we walk the unspecified parameters to ensure that they have the same default
            // values.

            for (var i = 0; i < signature1Parameters.Length; i++)
            {
                var parameter1 = signature1Parameters[i];
                if (specifiedParameters1.Contains(parameter1))
                {
                    continue;
                }

                var parameter2 = signature2Parameters[i];

                Debug.Assert(parameter1.HasExplicitDefaultValue, "Expected all unspecified parameter to have default values");
                Debug.Assert(parameter1.HasExplicitDefaultValue == parameter2.HasExplicitDefaultValue);

                if (parameter1.HasExplicitDefaultValue && parameter2.HasExplicitDefaultValue)
                {
                    if (!object.Equals(parameter2.ExplicitDefaultValue, parameter1.ExplicitDefaultValue))
                    {
                        return false;
                    }

                    if (object.Equals(parameter1.ExplicitDefaultValue, 0.0))
                    {
                        RoslynDebug.Assert(object.Equals(parameter2.ExplicitDefaultValue, 0.0));

                        var isParam1DefaultValueNegativeZero = double.IsNegativeInfinity(1.0 / (double)parameter1.ExplicitDefaultValue);
                        var isParam2DefaultValueNegativeZero = double.IsNegativeInfinity(1.0 / (double)parameter2.ExplicitDefaultValue);
                        if (isParam1DefaultValueNegativeZero != isParam2DefaultValueNegativeZero)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        protected void GetConversions(
            TExpressionSyntax originalExpression,
            ITypeSymbol originalTargetType,
            TExpressionSyntax newExpression,
            ITypeSymbol newTargetType,
            out TConversion? originalConversion,
            out TConversion? newConversion)
        {
            originalConversion = null;
            newConversion = null;

            if (this.OriginalSemanticModel.GetTypeInfo(originalExpression).Type != null &&
                this.SpeculativeSemanticModel.GetTypeInfo(newExpression).Type != null)
            {
                originalConversion = ClassifyConversion(this.OriginalSemanticModel, originalExpression, originalTargetType);
                newConversion = ClassifyConversion(this.SpeculativeSemanticModel, newExpression, newTargetType);
            }
            else
            {
                var originalConvertedTypeSymbol = this.OriginalSemanticModel.GetTypeInfo(originalExpression).ConvertedType;
                if (originalConvertedTypeSymbol != null)
                {
                    originalConversion = ClassifyConversion(this.OriginalSemanticModel, originalConvertedTypeSymbol, originalTargetType);
                }

                var newConvertedTypeSymbol = this.SpeculativeSemanticModel.GetTypeInfo(newExpression).ConvertedType;
                if (newConvertedTypeSymbol != null)
                {
                    newConversion = ClassifyConversion(this.SpeculativeSemanticModel, newConvertedTypeSymbol, newTargetType);
                }
            }
        }

        protected abstract TConversion ClassifyConversion(SemanticModel model, TExpressionSyntax expression, ITypeSymbol targetType);
        protected abstract TConversion ClassifyConversion(SemanticModel model, ITypeSymbol originalType, ITypeSymbol targetType);
    }
}
