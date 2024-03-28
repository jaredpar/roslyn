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
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.Cci
{
    internal sealed class PooledBlobBuilder : BlobBuilder, IDisposable
    {
        internal const int PoolSize = 1024 * 4;
        internal const int PoolChunkSize = 1024;
        internal const int PoolMaxChunkSize = 1024 * 16;

        private bool _rentsArray;

        protected override int? MaxChunkSize => PoolMaxChunkSize;

        private static readonly ObjectPool<PooledBlobBuilder> s_chunkPool = new ObjectPool<PooledBlobBuilder>(() => new PooledBlobBuilder(), PoolSize);

        private PooledBlobBuilder()
            : base(Array.Empty<byte>())
        {
            _rentsArray = false;
        }

        private PooledBlobBuilder(int size)
            : base(ArrayPool<byte>.Shared.Rent(size))
        {
            _rentsArray = true;
        }

        public static PooledBlobBuilder GetInstanceEx(int capacity) => GetInstance(capacity);

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
            var builder = s_chunkPool.Allocate();
            Debug.Assert(!builder._rentsArray);
            Debug.Assert(builder.Buffer.Length == 0);

            builder.Buffer = ArrayPool<byte>.Shared.Rent(capacity);
            builder._rentsArray = true;
            if (zero)
            {
                builder.Buffer.AsSpan().Clear();
            }
            return builder;
        }

        protected override BlobBuilder AllocateChunk(int minimalSize) => PooledBlobBuilder.GetInstance(minimalSize);

        public override void SetCapacityCore(int capacity)
        {
            if (_rentsArray)
            {
                var oldBuffer = Buffer;
                Buffer = ArrayPool<byte>.Shared.Rent(capacity);
                var copy = Math.Min(oldBuffer.Length, Buffer.Length);
                this.WriteBytes(oldBuffer.AsSpan().Slice(0, copy));
                ArrayPool<byte>.Shared.Return(oldBuffer);
            }
            else
            {
                base.SetCapacityCore(capacity);
            }
        }

        protected override void BeforeSwapCore(BlobBuilder other)
        {
            if (other is not PooledBlobBuilder { _rentsArray: true })
            {
                Debug.Assert(false);
                _rentsArray = false;
            }
        }

        protected override void FreeChunk()
        {
            if (_rentsArray)
            {
                ArrayPool<byte>.Shared.Return(Buffer);
                Buffer = Array.Empty<byte>();
                _rentsArray = false;
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
