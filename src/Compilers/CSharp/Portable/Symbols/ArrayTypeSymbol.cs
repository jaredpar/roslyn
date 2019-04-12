﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    /// <summary>
    /// An ArrayTypeSymbol represents an array type, such as int[] or object[,].
    /// </summary>
    internal abstract partial class ArrayTypeSymbol : TypeSymbol, IArrayTypeSymbol
    {
        private readonly TypeWithAnnotations _elementTypeWithAnnotations;
        private readonly NamedTypeSymbol _baseType;

        private ArrayTypeSymbol(
            TypeWithAnnotations elementTypeWithAnnotations,
            NamedTypeSymbol array)
        {
            Debug.Assert(elementTypeWithAnnotations.HasType);
            Debug.Assert((object)array != null);

            _elementTypeWithAnnotations = elementTypeWithAnnotations;
            _baseType = array;
        }

        internal static ArrayTypeSymbol CreateCSharpArray(
            AssemblySymbol declaringAssembly,
            TypeWithAnnotations elementTypeWithAnnotations,
            int rank = 1)
        {
            if (rank == 1)
            {
                return CreateSZArray(declaringAssembly, elementTypeWithAnnotations);
            }

            return CreateMDArray(declaringAssembly, elementTypeWithAnnotations, rank, default(ImmutableArray<int>), default(ImmutableArray<int>));
        }

        internal static ArrayTypeSymbol CreateMDArray(
            TypeWithAnnotations elementTypeWithAnnotations,
            int rank,
            ImmutableArray<int> sizes,
            ImmutableArray<int> lowerBounds,
            NamedTypeSymbol array)
        {
            // Optimize for most common case - no sizes and all dimensions are zero lower bound.
            if (sizes.IsDefaultOrEmpty && lowerBounds.IsDefault)
            {
                return new MDArrayNoSizesOrBounds(elementTypeWithAnnotations, rank, array);
            }

            return new MDArrayWithSizesAndBounds(elementTypeWithAnnotations, rank, sizes, lowerBounds, array);
        }

        internal static ArrayTypeSymbol CreateMDArray(
            AssemblySymbol declaringAssembly,
            TypeWithAnnotations elementType,
            int rank,
            ImmutableArray<int> sizes,
            ImmutableArray<int> lowerBounds)
        {
            return CreateMDArray(elementType, rank, sizes, lowerBounds, declaringAssembly.GetSpecialType(SpecialType.System_Array));
        }

        internal static ArrayTypeSymbol CreateSZArray(
            TypeWithAnnotations elementTypeWithAnnotations,
            NamedTypeSymbol array)
        {
            return new SZArray(elementTypeWithAnnotations, array, GetSZArrayInterfaces(elementTypeWithAnnotations, array.ContainingAssembly));
        }

        internal static ArrayTypeSymbol CreateSZArray(
            TypeWithAnnotations elementTypeWithAnnotations,
            NamedTypeSymbol array,
            ImmutableArray<NamedTypeSymbol> constructedInterfaces)
        {
            return new SZArray(elementTypeWithAnnotations, array, constructedInterfaces);
        }

        internal static ArrayTypeSymbol CreateSZArray(
            AssemblySymbol declaringAssembly,
            TypeWithAnnotations elementType)
        {
            return CreateSZArray(elementType, declaringAssembly.GetSpecialType(SpecialType.System_Array), GetSZArrayInterfaces(elementType, declaringAssembly));
        }

        internal ArrayTypeSymbol WithElementType(TypeWithAnnotations elementTypeWithAnnotations)
        {
            return ElementTypeWithAnnotations.IsSameAs(elementTypeWithAnnotations) ? this : WithElementTypeCore(elementTypeWithAnnotations);
        }

        protected abstract ArrayTypeSymbol WithElementTypeCore(TypeWithAnnotations elementTypeWithAnnotations);

        private static ImmutableArray<NamedTypeSymbol> GetSZArrayInterfaces(
            TypeWithAnnotations elementTypeWithAnnotations,
            AssemblySymbol declaringAssembly)
        {
            var constructedInterfaces = ArrayBuilder<NamedTypeSymbol>.GetInstance();

            //There are cases where the platform does contain the interfaces.
            //So it is fine not to have them listed under the type
            var iListOfT = declaringAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);
            if (!iListOfT.IsErrorType())
            {
                constructedInterfaces.Add(new ConstructedNamedTypeSymbol(iListOfT, ImmutableArray.Create(elementTypeWithAnnotations)));
            }

            var iReadOnlyListOfT = declaringAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyList_T);

            if (!iReadOnlyListOfT.IsErrorType())
            {
                constructedInterfaces.Add(new ConstructedNamedTypeSymbol(iReadOnlyListOfT, ImmutableArray.Create(elementTypeWithAnnotations)));
            }

            return constructedInterfaces.ToImmutableAndFree();
        }

        /// <summary>
        /// Gets the number of dimensions of the array. A regular single-dimensional array
        /// has rank 1, a two-dimensional array has rank 2, etc.
        /// </summary>
        public abstract int Rank { get; }

        /// <summary>
        /// Is this a zero-based one-dimensional array, i.e. SZArray in CLR terms.
        /// </summary>
        public abstract bool IsSZArray { get; }

        internal bool HasSameShapeAs(ArrayTypeSymbol other)
        {
            return Rank == other.Rank && IsSZArray == other.IsSZArray;
        }

        /// <summary>
        /// Specified sizes for dimensions, by position. The length can be less than <see cref="Rank"/>,
        /// meaning that some trailing dimensions don't have the size specified.
        /// The most common case is none of the dimensions have the size specified - an empty array is returned.
        /// </summary>
        public virtual ImmutableArray<int> Sizes
        {
            get
            {
                return ImmutableArray<int>.Empty;
            }
        }

        /// <summary>
        /// Specified lower bounds for dimensions, by position. The length can be less than <see cref="Rank"/>,
        /// meaning that some trailing dimensions don't have the lower bound specified.
        /// The most common case is all dimensions are zero bound - a default array is returned in this case.
        /// </summary>
        public virtual ImmutableArray<int> LowerBounds
        {
            get
            {
                return default(ImmutableArray<int>);
            }
        }

        /// <summary>
        /// Note, <see cref="Rank"/> equality should be checked separately!!!
        /// </summary>
        internal bool HasSameSizesAndLowerBoundsAs(ArrayTypeSymbol other)
        {
            if (this.Sizes.SequenceEqual(other.Sizes))
            {
                var thisLowerBounds = this.LowerBounds;

                if (thisLowerBounds.IsDefault)
                {
                    return other.LowerBounds.IsDefault;
                }

                var otherLowerBounds = other.LowerBounds;

                return !otherLowerBounds.IsDefault && thisLowerBounds.SequenceEqual(otherLowerBounds);
            }

            return false;
        }

        /// <summary>
        /// Normally C# arrays have default sizes and lower bounds - sizes are not specified and all dimensions are zero bound.
        /// This property should return false for any deviations.
        /// </summary>
        internal abstract bool HasDefaultSizesAndLowerBounds { get; }

        /// <summary>
        /// Gets the type of the elements stored in the array along with its annotations.
        /// </summary>
        public TypeWithAnnotations ElementTypeWithAnnotations
        {
            get
            {
                return _elementTypeWithAnnotations;
            }
        }

        /// <summary>
        /// Gets the type of the elements stored in the array.
        /// </summary>
        public TypeSymbol ElementType
        {
            get
            {
                return _elementTypeWithAnnotations.Type;
            }
        }

        internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => _baseType;

        public override bool IsReferenceType
        {
            get
            {
                return true;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }

        internal sealed override ManagedKind ManagedKind => ManagedKind.Managed;

        public sealed override bool IsRefLikeType
        {
            get
            {
                return false;
            }
        }

        public sealed override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        internal sealed override ObsoleteAttributeData ObsoleteAttributeData
        {
            get { return null; }
        }

        public override ImmutableArray<Symbol> GetMembers()
        {
            return ImmutableArray<Symbol>.Empty;
        }

        public override ImmutableArray<Symbol> GetMembers(string name)
        {
            return ImmutableArray<Symbol>.Empty;
        }

        public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
        {
            return ImmutableArray<NamedTypeSymbol>.Empty;
        }

        public override SymbolKind Kind
        {
            get
            {
                return SymbolKind.ArrayType;
            }
        }

        public override TypeKind TypeKind
        {
            get
            {
                return TypeKind.Array;
            }
        }

        public override Symbol ContainingSymbol
        {
            get
            {
                return null;
            }
        }

        public override ImmutableArray<Location> Locations
        {
            get
            {
                return ImmutableArray<Location>.Empty;
            }
        }

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
        {
            get
            {
                return ImmutableArray<SyntaxReference>.Empty;
            }
        }

        internal override TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitArrayType(this, argument);
        }

        public override void Accept(CSharpSymbolVisitor visitor)
        {
            visitor.VisitArrayType(this);
        }

        public override TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor)
        {
            return visitor.VisitArrayType(this);
        }

        internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
        {
            return this.Equals(t2 as ArrayTypeSymbol, comparison);
        }

        internal bool Equals(ArrayTypeSymbol other)
        {
            return Equals(other, TypeCompareKind.ConsiderEverything);
        }

        private bool Equals(ArrayTypeSymbol other, TypeCompareKind comparison)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // We don't want to blow the stack if we have a type like T[][][][][][][][]....[][],
            // so we do not recurse until we have a non-array. Rather, check all the ranks
            // and then check the final "T" type.
            var array = this;
            do
            {
                if (other is null || !other.HasSameShapeAs(array))
                {
                    return false;
                }

                // Make sure bounds are the same
                if ((comparison & TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds) == 0 && !array.HasSameSizesAndLowerBoundsAs(other))
                {
                    return false;
                }

                var arrayElementType = array.ElementTypeWithAnnotations;
                var otherElementType = other.ElementTypeWithAnnotations;
                if (arrayElementType.IsSZArray())
                {
                    // Compare everything but the actual ArrayTypeSymbol instance. 
                    var otherTwa = TypeWithAnnotations.Create(arrayElementType.Type, otherElementType.NullableAnnotation, otherElementType.CustomModifiers);
                    if (!arrayElementType.Equals(otherTwa, comparison))
                    {
                        return false;
                    }

                    array = (ArrayTypeSymbol)arrayElementType.Type;
                    other = otherElementType.Type as ArrayTypeSymbol;
                }
                else
                {
                    return arrayElementType.Equals(otherElementType, comparison);
                }
            }
            while (true);
        }

        public override int GetHashCode()
        {
            // We don't want to blow the stack if we have a type like T[][][][][][][][]....[][],
            // so we do not recurse until we have a non-array. Rather, hash all the ranks together
            // and then hash that with the "T" type.

            int hash = 0;
            TypeSymbol current = this;
            while (current.TypeKind == TypeKind.Array)
            {
                var cur = (ArrayTypeSymbol)current;
                hash = Hash.Combine(cur.Rank, hash);
                current = cur.ElementType;
            }

            return Hash.Combine(current, hash);
        }

        internal override void AddNullableTransforms(ArrayBuilder<byte> transforms)
        {
            var elementType = ElementTypeWithAnnotations;
            while (elementType.Type is ArrayTypeSymbol arrayType)
            {
                elementType.AddNullableTransformShallow(transforms);
                elementType = arrayType.ElementTypeWithAnnotations;
            }

            elementType.AddNullableTransforms(transforms);
        }

        internal override TypeSymbol ApplyNullableTransforms(NullableTransformStream stream)
        {
            // The language supports deeply nested arrays, up to 10,000+ instances. This means we can't implemented a head
            // recursive solution here. Need to take an iterative approach.
            var builder = ArrayBuilder<(ArrayTypeSymbol array, byte? transform)>.GetInstance();

            // First build up the state for each of the ArrayTypeSymbol in the chain of arrays.
            var array = this;
            builder.Push((this, stream.GetNextTransform()));
            do
            {
                array = array.ElementType as ArrayTypeSymbol;
                if (array is null)
                {
                    break;
                }
                else
                {
                    builder.Push((array, stream.GetNextTransform()));
                }
            }
            while (true);

            // Create the initial return for the most nested array. 
            var lastState = builder.Pop();
            var lastElementType = apply(
                lastState.array.ElementTypeWithAnnotations,
                lastState.array.ElementTypeWithAnnotations.Type.ApplyNullableTransforms(stream),
                lastState.transform);
            var finalArray = lastState.array.WithElementType(lastElementType);

            // Unwind the stack and rebuild the array chain.
            while (builder.Count > 0)
            {
                var state = builder.Pop();
                var elementType = apply(state.array.ElementTypeWithAnnotations, finalArray, state.transform);
                finalArray = state.array.WithElementType(elementType);
            }

            builder.Free();
            return finalArray;

            TypeWithAnnotations apply(TypeWithAnnotations twa, TypeSymbol type, byte? transform)
            {
                twa = TypeWithAnnotations.Create(type, twa.NullableAnnotation, twa.CustomModifiers);
                if (transform.HasValue &&
                    twa.ApplyNullableTransformShallow(transform.Value) is TypeWithAnnotations other)
                {
                    return other;
                }
                else
                {
                    stream.SetHasInsufficientData();
                    return twa;
                }
            }
        }

        internal override TypeSymbol SetObliviousNullabilityForReferenceTypes()
        {
            // The language supports deeply nested arrays, up to 10,000+ instances. This means we can't implemented a head
            // recursive solution here. Need to take an iterative approach.
            var builder = ArrayBuilder<ArrayTypeSymbol>.GetInstance();

            // First build up the state for each of the ArrayTypeSymbol in the chain of arrays.
            var array = this;
            builder.Push(array);
            do
            {
                array = array.ElementType as ArrayTypeSymbol;
                if (array is null)
                {
                    break;
                }
                else
                {
                    builder.Push(array);
                }
            }
            while (true);

            // Create the initial return for the most nested array. 
            var finalArray = builder.Pop();
            finalArray = finalArray.WithElementType(finalArray.ElementTypeWithAnnotations.SetObliviousNullabilityForReferenceTypes());

            // Unwind the stack and rebuild the array chain.
            while (builder.Count > 0)
            {
                var state = builder.Pop();
                var elementType = TypeWithAnnotations.Create(finalArray, NullableAnnotation.Oblivious, state.ElementTypeWithAnnotations.CustomModifiers);
                finalArray = state.WithElementType(elementType);
            }

            builder.Free();
            return finalArray;
        }

        internal override TypeSymbol MergeNullability(TypeSymbol other, VarianceKind variance)
        {
            Debug.Assert(this.Equals(other, TypeCompareKind.IgnoreDynamicAndTupleNames | TypeCompareKind.IgnoreNullableModifiersForReferenceTypes));
            var builder = ArrayBuilder<(ArrayTypeSymbol thisArray, ArrayTypeSymbol otherArray)>.GetInstance();

            // First build up the pair of arrays that need to be processed.
            var state = (thisArray: this, otherArray: (ArrayTypeSymbol)other);
            builder.Push(state);
            do
            {
                var nextArray = state.thisArray.ElementType as ArrayTypeSymbol;
                if (nextArray is null)
                {
                    break;
                }
                else
                {
                    state = (nextArray, (ArrayTypeSymbol)state.otherArray.ElementType);
                    builder.Push(state);
                }
            }
            while (true);

            // Next build up the most nested element.
            var lastState = builder.Pop();
            var lastElementType = lastState.thisArray.ElementTypeWithAnnotations.MergeNullability(
                lastState.otherArray.ElementTypeWithAnnotations,
                variance);
            var finalArray = lastState.thisArray.WithElementType(lastElementType);

            // Unwind the stack and rebuild the array chain.
            while (builder.Count > 0)
            {
                state = builder.Pop();
                var nullableAnnotation = NullableAnnotationExtensions.MergeNullableAnnotation(
                    state.thisArray.ElementTypeWithAnnotations.NullableAnnotation,
                    state.otherArray.ElementTypeWithAnnotations.NullableAnnotation,
                    variance);
                var elementType = TypeWithAnnotations.Create(finalArray, nullableAnnotation, state.thisArray.ElementTypeWithAnnotations.CustomModifiers);
                finalArray = state.thisArray.WithElementType(elementType);
            }

            return finalArray;
        }

        public override Accessibility DeclaredAccessibility
        {
            get
            {
                return Accessibility.NotApplicable;
            }
        }

        public override bool IsStatic
        {
            get
            {
                return false;
            }
        }

        public override bool IsAbstract
        {
            get
            {
                return false;
            }
        }

        public override bool IsSealed
        {
            get
            {
                return false;
            }
        }

        #region Use-Site Diagnostics

        internal override DiagnosticInfo GetUseSiteDiagnostic()
        {
            DiagnosticInfo result = null;

            // check element type
            // check custom modifiers
            if (DeriveUseSiteDiagnosticFromType(ref result, this.ElementTypeWithAnnotations))
            {
                return result;
            }

            return result;
        }

        internal override bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
        {
            return _elementTypeWithAnnotations.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes) ||
                   ((object)_baseType != null && _baseType.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes)) ||
                   GetUnificationUseSiteDiagnosticRecursive(ref result, this.InterfacesNoUseSiteDiagnostics(), owner, ref checkedTypes);
        }

        #endregion

        #region IArrayTypeSymbol Members

        ITypeSymbol IArrayTypeSymbol.ElementType
        {
            get { return this.ElementType; }
        }

        ImmutableArray<CustomModifier> IArrayTypeSymbol.CustomModifiers
        {
            get { return this.ElementTypeWithAnnotations.CustomModifiers; }
        }

        bool IArrayTypeSymbol.Equals(IArrayTypeSymbol symbol)
        {
            return this.Equals(symbol as ArrayTypeSymbol);
        }

        #endregion

        #region ISymbol Members

        public override void Accept(SymbolVisitor visitor)
        {
            visitor.VisitArrayType(this);
        }

        public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            return visitor.VisitArrayType(this);
        }

        #endregion

        /// <summary>
        /// Represents SZARRAY - zero-based one-dimensional array 
        /// </summary>
        private sealed class SZArray : ArrayTypeSymbol
        {
            private readonly ImmutableArray<NamedTypeSymbol> _interfaces;

            internal SZArray(
                TypeWithAnnotations elementTypeWithAnnotations,
                NamedTypeSymbol array,
                ImmutableArray<NamedTypeSymbol> constructedInterfaces)
                : base(elementTypeWithAnnotations, array)
            {
                Debug.Assert(constructedInterfaces.Length <= 2);
                _interfaces = constructedInterfaces;
            }

            protected override ArrayTypeSymbol WithElementTypeCore(TypeWithAnnotations newElementType)
            {
                var newInterfaces = _interfaces.SelectAsArray((i, t) => i.OriginalDefinition.Construct(t), newElementType.Type);
                return new SZArray(newElementType, BaseTypeNoUseSiteDiagnostics, newInterfaces);
            }

            public override int Rank
            {
                get
                {
                    return 1;
                }
            }

            /// <summary>
            /// SZArray is an array type encoded in metadata with ELEMENT_TYPE_SZARRAY (always single-dim array with 0 lower bound).
            /// Non-SZArray type is encoded in metadata with ELEMENT_TYPE_ARRAY and with optional sizes and lower bounds. Even though 
            /// non-SZArray can also be a single-dim array with 0 lower bound, the encoding of these types in metadata is distinct.
            /// </summary>
            public override bool IsSZArray
            {
                get
                {
                    return true;
                }
            }

            internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved = null)
            {
                return _interfaces;
            }

            internal override bool HasDefaultSizesAndLowerBounds
            {
                get
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Represents MDARRAY - multi-dimensional array (possibly of rank 1)
        /// </summary>
        private abstract class MDArray : ArrayTypeSymbol
        {
            private readonly int _rank;

            internal MDArray(
                TypeWithAnnotations elementTypeWithAnnotations,
                int rank,
                NamedTypeSymbol array)
                : base(elementTypeWithAnnotations, array)
            {
                Debug.Assert(rank >= 1);
                _rank = rank;
            }

            public sealed override int Rank
            {
                get
                {
                    return _rank;
                }
            }

            public sealed override bool IsSZArray
            {
                get
                {
                    return false;
                }
            }

            internal sealed override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<TypeSymbol> basesBeingResolved = null)
            {
                return ImmutableArray<NamedTypeSymbol>.Empty;
            }
        }

        private sealed class MDArrayNoSizesOrBounds : MDArray
        {
            internal MDArrayNoSizesOrBounds(
                TypeWithAnnotations elementTypeWithAnnotations,
                int rank,
                NamedTypeSymbol array)
                : base(elementTypeWithAnnotations, rank, array)
            {
            }

            protected override ArrayTypeSymbol WithElementTypeCore(TypeWithAnnotations elementTypeWithAnnotations)
            {
                return new MDArrayNoSizesOrBounds(elementTypeWithAnnotations, Rank, BaseTypeNoUseSiteDiagnostics);
            }

            internal override bool HasDefaultSizesAndLowerBounds
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class MDArrayWithSizesAndBounds : MDArray
        {
            private readonly ImmutableArray<int> _sizes;
            private readonly ImmutableArray<int> _lowerBounds;

            internal MDArrayWithSizesAndBounds(
                TypeWithAnnotations elementTypeWithAnnotations,
                int rank,
                ImmutableArray<int> sizes,
                ImmutableArray<int> lowerBounds,
                NamedTypeSymbol array)
                : base(elementTypeWithAnnotations, rank, array)
            {
                Debug.Assert(!sizes.IsDefaultOrEmpty || !lowerBounds.IsDefault);
                Debug.Assert(lowerBounds.IsDefaultOrEmpty || (!lowerBounds.IsEmpty && (lowerBounds.Length != rank || !lowerBounds.All(b => b == 0))));
                _sizes = sizes.NullToEmpty();
                _lowerBounds = lowerBounds;
            }

            protected override ArrayTypeSymbol WithElementTypeCore(TypeWithAnnotations elementTypeWithAnnotations)
            {
                return new MDArrayWithSizesAndBounds(elementTypeWithAnnotations, Rank, _sizes, _lowerBounds, BaseTypeNoUseSiteDiagnostics);
            }

            public override ImmutableArray<int> Sizes
            {
                get
                {
                    return _sizes;
                }
            }

            public override ImmutableArray<int> LowerBounds
            {
                get
                {
                    return _lowerBounds;
                }
            }

            internal override bool HasDefaultSizesAndLowerBounds
            {
                get
                {
                    return false;
                }
            }
        }
    }
}
