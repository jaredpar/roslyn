// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

        private bool _rentsArray;

        private PooledBlobBuilder(int size)
            : base(ArrayPool<byte>.Shared.Rent(size))
        {
            _rentsArray = true;
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
        public static PooledBlobBuilder GetInstance(int capacity = PoolChunkSize, bool zero = false)
        {
            var builder = new PooledBlobBuilder(capacity);
            if (zero)
            {
                builder._buffer.AsSpan().Clear();
            }

            return builder;
        }

        protected override byte[] AllocateBytes(int size) => throw new Exception("should not get here");

        protected override BlobBuilder AllocateChunk(int minimalSize)
        {
            return new PooledBlobBuilder(minimalSize);
        }

        protected override void LinkThisToOther(BlobBuilder other, bool isSuffix)
        {
            if (other is not PooledBlobBuilder)
            {
                _rentsArray = false;
            }
        }

        protected override void LinkOtherToThis(BlobBuilder @this, bool isSuffix)
        {
            if (@this is not PooledBlobBuilder)
            {
                _rentsArray = false;
            }
        }

        protected override void FreeChunk()
        {
            if (_rentsArray)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }

            _buffer = null!;
            _rentsArray = false;
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

        internal int WriteBytesSegmented(Stream stream)
        {
            Debug.Assert(stream.CanSeek);
            Debug.Assert(stream.CanRead);

            Span<byte> buffer = stackalloc byte[PoolChunkSize];
            var written = 0;
            do
            {
                var read = stream.Read(buffer);
                if (read == 0)
                {
                    break;
                }

                WriteBytesSegmented(buffer.Slice(0, read));
                written += read;
            } while (true);

            return written;
        }

        internal int WriteBytesSegmented(Stream stream, int byteCount)
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
