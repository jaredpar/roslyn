// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.Cci
{
    internal sealed class PooledBlobBuilder : BlobBuilder, IDisposable
    {
        internal const int PoolSize = 128;
        internal const int PoolChunkSize = 1024;

        private static readonly ObjectPool<PooledBlobBuilder> s_chunkPool = new ObjectPool<PooledBlobBuilder>(() => new PooledBlobBuilder(PoolChunkSize), PoolSize);

        private PooledBlobBuilder(int size)
            : base(size)
        {
        }

        /// <summary>
        /// Get a new instance of the <see cref="BlobBuilder"/> that has <see cref="BlobBuilder.ChunkCapacity"/> of
        /// at least <see cref="PoolChunkSize"/>
        /// </summary>
        /// <param name="zero">When true force zero out the backing buffer</param>
        /// <remarks>
        /// The <paramref name="zero"/> can be removed when moving to SRM 9.0 if it contains the bug fix for
        /// <see cref="BlobBuilder.ReserveBytes(int)"/>
        ///
        /// https://github.com/dotnet/runtime/issues/99244
        /// </remarks>
        public static PooledBlobBuilder GetInstance(bool zero = false)
        {
            var builder = s_chunkPool.Allocate();
            if (zero)
            {
                builder.WriteBytes(0, builder.ChunkCapacity);
                builder.Clear();
            }
            return builder;
        }

        protected override BlobBuilder AllocateChunk(int minimalSize)
        {
            if (minimalSize <= PoolChunkSize)
            {
                return s_chunkPool.Allocate();
            }

            return new BlobBuilder(minimalSize);
        }

        protected override void FreeChunk()
        {
            if (ChunkCapacity != PoolChunkSize)
            {
                // The invariant of this builder is that it produces BlobBuilder instances that have a 
                // ChunkCapacity of exactly 1024. Essentially inside AllocateChuck the pool must be able
                // to mindlessly allocate a BlobBuilder where ChunkCapacity is at least 1024.
                //
                // To maintain this the code must verify that the returned BlobBuilder instances have 
                // a backing array of the appropriate size. This array can shrink in practice through code
                // like the following: 
                //
                //      var builder = PooledBlobBuilder.GetInstance();
                //      builder.LinkSuffix(new BlobBuilder(256));
                //      builder.Free(); // calls FreeChunk where ChunkCapacity is 256
                //
                // This shouldn't happen much in practice due to convention of how builders are used but
                // it is a legal use of the APIs and must be accounted for.
                s_chunkPool.ForgetTrackedObject(this);
            }
            else
            {
                s_chunkPool.Free(this);
            }
        }

        internal void WriteBytesSegmented(ReadOnlySpan<byte> buffer)
        {
            while (buffer.Length > 0)
            {
                var maxWrite = ChunkCapacity == 0
                    ? PoolChunkSize
                    : ChunkCapacity;
                var toWrite = Math.Min(maxWrite, buffer.Length);
                this.WriteBytes(buffer.Slice(0, toWrite));
                buffer = buffer.Slice(toWrite);
            }
        }

        internal int TryWriteBytesSegmented(Stream stream, int byteCount)
        {
            Debug.Assert(stream.CanSeek);
            Debug.Assert(stream.CanRead);

            if (byteCount == 0)
            {
                return 0;
            }

            Span<byte> buffer = stackalloc byte[PoolChunkSize];
            var written = 0;
            do
            {
                var toRead = Math.Min(byteCount, buffer.Length);
                var read = stream.Read(buffer.Slice(0, toRead));
                if (read == 0)
                {
                    break;
                }

                WriteBytesSegmented(buffer.Slice(0, read));
                written += read;
            } while (written < byteCount);

            return written;
        }

        public new void Free()
        {
            base.Free();
        }

        void IDisposable.Dispose()
        {
            Free();
        }
    }
}
