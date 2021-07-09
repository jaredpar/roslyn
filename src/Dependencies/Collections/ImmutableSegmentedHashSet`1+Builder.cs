﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Collections
{
    internal readonly partial struct ImmutableSegmentedHashSet<T>
    {
        public sealed class Builder : ISet<T>, IReadOnlyCollection<T>
        {
            /// <summary>
            /// The immutable collection this builder is based on.
            /// </summary>
            private ImmutableSegmentedHashSet<T> _set;

            /// <summary>
            /// The current mutable collection this builder is operating on. This field is initialized to a copy of
            /// <see cref="_set"/> the first time a change is made.
            /// </summary>
            private SegmentedHashSet<T>? _mutableSet;

            internal Builder(ImmutableSegmentedHashSet<T> set)
            {
                _set = set;
                _mutableSet = null;
            }

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.KeyComparer"/>
            public IEqualityComparer<T> KeyComparer => ReadOnlySet.Comparer;

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.Count"/>
            public int Count => ReadOnlySet.Count;

            private SegmentedHashSet<T> ReadOnlySet => _mutableSet ?? _set._set;

            bool ICollection<T>.IsReadOnly => false;

            private SegmentedHashSet<T> GetOrCreateMutableSet()
            {
                if (_mutableSet is null)
                {
                    var originalSet = RoslynImmutableInterlocked.InterlockedExchange(ref _set, default);
                    if (originalSet.IsDefault)
                        throw new InvalidOperationException($"Unexpected concurrent access to {GetType()}");

                    _mutableSet = new SegmentedHashSet<T>(originalSet._set, originalSet.KeyComparer);
                }

                return _mutableSet;
            }

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.Add(T)"/>
            public bool Add(T item)
            {
                if (_mutableSet is null && Contains(item))
                    return false;

                return GetOrCreateMutableSet().Add(item);
            }

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.Clear()"/>
            public void Clear()
            {
                if (ReadOnlySet.Count != 0)
                {
                    if (_mutableSet is null)
                    {
                        _mutableSet = new SegmentedHashSet<T>(KeyComparer);
                        _set = default;
                    }
                    else
                    {
                        _mutableSet.Clear();
                    }
                }
            }

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.Contains(T)"/>
            public bool Contains(T item)
                => ReadOnlySet.Contains(item);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.ExceptWith(IEnumerable{T})"/>
            public void ExceptWith(IEnumerable<T> other)
                => GetOrCreateMutableSet().ExceptWith(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.GetEnumerator()"/>
            public Enumerator GetEnumerator()
                => new Enumerator(GetOrCreateMutableSet());

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.IntersectWith(IEnumerable{T})"/>
            public void IntersectWith(IEnumerable<T> other)
                => GetOrCreateMutableSet().IntersectWith(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.IsProperSubsetOf(IEnumerable{T})"/>
            public bool IsProperSubsetOf(IEnumerable<T> other)
                => ReadOnlySet.IsProperSubsetOf(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.IsProperSupersetOf(IEnumerable{T})"/>
            public bool IsProperSupersetOf(IEnumerable<T> other)
                => ReadOnlySet.IsProperSupersetOf(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.IsSubsetOf(IEnumerable{T})"/>
            public bool IsSubsetOf(IEnumerable<T> other)
                => ReadOnlySet.IsSubsetOf(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.IsSupersetOf(IEnumerable{T})"/>
            public bool IsSupersetOf(IEnumerable<T> other)
                => ReadOnlySet.IsSupersetOf(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.Overlaps(IEnumerable{T})"/>
            public bool Overlaps(IEnumerable<T> other)
                => ReadOnlySet.Overlaps(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.Remove(T)"/>
            public bool Remove(T item)
            {
                if (_mutableSet is null && !Contains(item))
                    return false;

                return GetOrCreateMutableSet().Remove(item);
            }

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.SetEquals(IEnumerable{T})"/>
            public bool SetEquals(IEnumerable<T> other)
                => ReadOnlySet.SetEquals(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.SymmetricExceptWith(IEnumerable{T})"/>
            public void SymmetricExceptWith(IEnumerable<T> other)
                => GetOrCreateMutableSet().SymmetricExceptWith(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.UnionWith(IEnumerable{T})"/>
            public void UnionWith(IEnumerable<T> other)
                => GetOrCreateMutableSet().UnionWith(other);

            /// <inheritdoc cref="ImmutableHashSet{T}.Builder.ToImmutable()"/>
            public ImmutableSegmentedHashSet<T> ToImmutable()
            {
                _set = new ImmutableSegmentedHashSet<T>(ReadOnlySet);
                _mutableSet = null;
                return _set;
            }

            void ICollection<T>.Add(T item)
                => ((ICollection<T>)GetOrCreateMutableSet()).Add(item);

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
                => ((ICollection<T>)ReadOnlySet).CopyTo(array, arrayIndex);

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
                => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
}
