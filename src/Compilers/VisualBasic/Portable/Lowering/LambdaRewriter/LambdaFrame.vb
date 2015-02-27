﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports Microsoft.CodeAnalysis.CodeGen
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols

Namespace Microsoft.CodeAnalysis.VisualBasic

    ''' <summary>
    ''' A class that represents the set of variables in a scope that have been
    ''' captured by lambdas within that scope.
    ''' </summary>
    Friend NotInheritable Class LambdaFrame
        Inherits SynthesizedContainer
        Implements ISynthesizedMethodBodyImplementationSymbol

        Private ReadOnly _typeParameters As ImmutableArray(Of TypeParameterSymbol)
        Private ReadOnly _topLevelMethod As MethodSymbol
        Private ReadOnly _sharedConstructor As MethodSymbol
        Private ReadOnly _singletonCache As FieldSymbol

        'NOTE: this does not include captured parent frame references 
        Friend ReadOnly m_captured_locals As New ArrayBuilder(Of LambdaCapturedVariable)
        Friend ReadOnly m_constructor As SynthesizedLambdaConstructor
        Friend ReadOnly TypeMap As TypeSubstitution

        Private ReadOnly _scopeSyntaxOpt As VisualBasicSyntaxNode

        Private Shared ReadOnly s_typeSubstitutionFactory As Func(Of Symbol, TypeSubstitution) =
            Function(container)
                Dim f = TryCast(container, LambdaFrame)
                Return If(f IsNot Nothing, f.TypeMap, DirectCast(container, SynthesizedMethod).TypeMap)
            End Function

        Friend Shared ReadOnly CreateTypeParameter As Func(Of TypeParameterSymbol, Symbol, TypeParameterSymbol) =
            Function(typeParameter, container) New SynthesizedClonedTypeParameterSymbol(typeParameter,
                                                                                        container,
                                                                                        GeneratedNames.MakeDisplayClassGenericParameterName(typeParameter.Ordinal),
                                                                                        s_typeSubstitutionFactory)
        Friend Sub New(slotAllocatorOpt As VariableSlotAllocator,
                       compilationState As TypeCompilationState,
                       topLevelMethod As MethodSymbol,
                       methodOrdinal As Integer,
                       scopeSyntax As VisualBasicSyntaxNode,
                       scopeOrdinal As Integer,
                       copyConstructor As Boolean,
                       isStatic As Boolean,
                       isDelegateRelaxationFrame As Boolean)

            MyBase.New(topLevelMethod, MakeName(slotAllocatorOpt, compilationState, methodOrdinal, scopeOrdinal, isStatic, isDelegateRelaxationFrame), topLevelMethod.ContainingType, ImmutableArray(Of NamedTypeSymbol).Empty)

            If copyConstructor Then
                Me.m_constructor = New SynthesizedLambdaCopyConstructor(scopeSyntax, Me)
            Else
                Me.m_constructor = New SynthesizedLambdaConstructor(scopeSyntax, Me)
            End If

            ' static lambdas technically have the class scope so the scope syntax is Nothing 
            If isStatic Then
                Me._sharedConstructor = New SynthesizedConstructorSymbol(Nothing, Me, isShared:=True, isDebuggable:=False, binder:=Nothing, diagnostics:=Nothing)
                Dim cacheVariableName = GeneratedNames.MakeCachedFrameInstanceName()
                Me._singletonCache = New SynthesizedFieldSymbol(Me, Me, Me, cacheVariableName, Accessibility.Public, isReadOnly:=True, isShared:=True)
                _scopeSyntaxOpt = Nothing
            Else
                _scopeSyntaxOpt = scopeSyntax
            End If

            AssertIsLambdaScopeSyntax(_scopeSyntaxOpt)

            Me._typeParameters = SynthesizedClonedTypeParameterSymbol.MakeTypeParameters(topLevelMethod.TypeParameters, Me, CreateTypeParameter)
            Me.TypeMap = TypeSubstitution.Create(topLevelMethod, topLevelMethod.TypeParameters, Me.TypeArgumentsNoUseSiteDiagnostics)
            Me._topLevelMethod = topLevelMethod
        End Sub

        Private Shared Function MakeName(slotAllocatorOpt As VariableSlotAllocator,
                                         compilationState As TypeCompilationState,
                                         methodOrdinal As Integer,
                                         scopeOrdinal As Integer,
                                         isStatic As Boolean,
                                         isDelegateRelaxation As Boolean) As String

            ' Note: module builder is not available only when testing emit diagnostics
            Dim generation = If(compilationState.ModuleBuilderOpt?.CurrentGenerationOrdinal, 0)

            If isStatic Then
                Debug.Assert(methodOrdinal >= -1)
                Return GeneratedNames.MakeStaticLambdaDisplayClassName(methodOrdinal, generation)
            End If

            Debug.Assert(methodOrdinal >= 0)
            Return GeneratedNames.MakeLambdaDisplayClassName(methodOrdinal, generation, scopeOrdinal, isDelegateRelaxation)
        End Function

        <Conditional("DEBUG")>
        Private Shared Sub AssertIsLambdaScopeSyntax(syntax As VisualBasicSyntaxNode)
            ' TODO:
        End Sub

        Public ReadOnly Property ScopeSyntax As VisualBasicSyntaxNode
            Get
                Return m_constructor.Syntax
            End Get
        End Property

        Public Overrides ReadOnly Property DeclaredAccessibility As Accessibility
            Get
                ' Dev11 uses "assembly" here. No need to be different.
                Return Accessibility.Friend
            End Get
        End Property

        Public Overloads Overrides Function GetMembers(name As String) As ImmutableArray(Of Symbol)
            Return ImmutableArray(Of Symbol).Empty
        End Function

        Public Overloads Overrides Function GetMembers() As ImmutableArray(Of Symbol)
            Dim members = StaticCast(Of Symbol).From(m_captured_locals.AsImmutable())
            If _sharedConstructor IsNot Nothing Then
                members = members.AddRange(ImmutableArray.Create(Of Symbol)(m_constructor, _sharedConstructor, _singletonCache))
            Else
                members = members.Add(m_constructor)
            End If

            Return members
        End Function

        Protected Friend Overrides ReadOnly Property Constructor As MethodSymbol
            Get
                Return m_constructor
            End Get
        End Property

        Protected Friend ReadOnly Property SharedConstructor As MethodSymbol
            Get
                Return _sharedConstructor
            End Get
        End Property

        Friend ReadOnly Property SingletonCache As FieldSymbol
            Get
                Return _singletonCache
            End Get
        End Property

        Friend Overrides ReadOnly Property IsSerializable As Boolean
            Get
                Return _singletonCache IsNot Nothing
            End Get
        End Property


        Friend Overrides Function GetFieldsToEmit() As IEnumerable(Of FieldSymbol)
            If _singletonCache Is Nothing Then
                Return m_captured_locals
            Else
                Return DirectCast(m_captured_locals, IEnumerable(Of FieldSymbol)).Concat(Me._singletonCache)
            End If
        End Function

        Friend Overrides Function MakeAcyclicBaseType(diagnostics As DiagnosticBag) As NamedTypeSymbol
            Dim type = ContainingAssembly.GetSpecialType(SpecialType.System_Object)
            ' WARN: We assume that if System_Object was not found we would never reach 
            '       this point because the error should have been/processed generated earlier
            Debug.Assert(type.GetUseSiteErrorInfo() Is Nothing)
            Return type
        End Function

        Friend Overrides Function MakeAcyclicInterfaces(diagnostics As DiagnosticBag) As ImmutableArray(Of NamedTypeSymbol)
            Return ImmutableArray(Of NamedTypeSymbol).Empty
        End Function

        Friend Overrides Function MakeDeclaredBase(basesBeingResolved As ConsList(Of Symbol), diagnostics As DiagnosticBag) As NamedTypeSymbol
            Dim type = ContainingAssembly.GetSpecialType(SpecialType.System_Object)
            ' WARN: We assume that if System_Object was not found we would never reach 
            '       this point because the error should have been/processed generated earlier
            Debug.Assert(type.GetUseSiteErrorInfo() Is Nothing)
            Return type
        End Function

        Friend Overrides Function MakeDeclaredInterfaces(basesBeingResolved As ConsList(Of Symbol), diagnostics As DiagnosticBag) As ImmutableArray(Of NamedTypeSymbol)
            Return ImmutableArray(Of NamedTypeSymbol).Empty
        End Function

        Public Overrides ReadOnly Property MemberNames As IEnumerable(Of String)
            Get
                Return SpecializedCollections.EmptyEnumerable(Of String)()
            End Get
        End Property

        Public Overrides ReadOnly Property TypeKind As TypeKind
            Get
                Return TypeKind.Class
            End Get
        End Property

        Public Overrides ReadOnly Property Arity As Integer
            Get
                Return Me._typeParameters.Length
            End Get
        End Property

        Public Overrides ReadOnly Property TypeParameters As ImmutableArray(Of TypeParameterSymbol)
            Get
                Return Me._typeParameters
            End Get
        End Property

        Friend Overrides ReadOnly Property IsInterface As Boolean
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property HasMethodBodyDependency As Boolean Implements ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency
            Get
                ' This method contains user code from the lamda
                Return True
            End Get
        End Property

        Public ReadOnly Property Method As IMethodSymbol Implements ISynthesizedMethodBodyImplementationSymbol.Method
            Get
                Return _topLevelMethod
            End Get
        End Property
    End Class

End Namespace
