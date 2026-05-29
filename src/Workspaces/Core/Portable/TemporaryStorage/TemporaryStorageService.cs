// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Host;

/// <summary>
/// Temporarily stores text and streams in memory mapped files.
/// </summary>
#if NET
[SupportedOSPlatform("windows")]
#endif
internal sealed partial class TemporaryStorageService : ITemporaryStorageServiceInternal
{
    private readonly IWorkspaceThreadingService? _workspaceThreadingService;
    private readonly ITextFactoryService _textFactory;

    [Obsolete(MefConstruction.FactoryMethodMessage, error: true)]
    private TemporaryStorageService(IWorkspaceThreadingService? workspaceThreadingService, ITextFactoryService textFactory)
    {
        _workspaceThreadingService = workspaceThreadingService;
        _textFactory = textFactory;
    }

    ITemporaryStorageTextHandle ITemporaryStorageServiceInternal.WriteToTemporaryStorage(SourceText text, CancellationToken cancellationToken)
        => WriteToTemporaryStorage(text, cancellationToken);

    async Task<ITemporaryStorageTextHandle> ITemporaryStorageServiceInternal.WriteToTemporaryStorageAsync(SourceText text, CancellationToken cancellationToken)
        => await WriteToTemporaryStorageAsync(text, cancellationToken).ConfigureAwait(false);

    public TemporaryStorageTextHandle WriteToTemporaryStorage(SourceText text, CancellationToken cancellationToken)
    {
        var memoryMappedInfo = WriteToMemoryMappedFile();
        var identifier = new TemporaryStorageIdentifier(memoryMappedInfo.Name, memoryMappedInfo.Offset, memoryMappedInfo.Size);
        return new(this, memoryMappedInfo.MemoryMappedFile, identifier, text.ChecksumAlgorithm, text.Encoding, text.GetContentHash());

        MemoryMappedInfo WriteToMemoryMappedFile()
        {
            using (Logger.LogBlock(FunctionId.TemporaryStorageServiceFactory_WriteText, cancellationToken))
            {
                // the method we use to get text out of SourceText uses Unicode (2bytes per char). 
                var size = Encoding.Unicode.GetMaxByteCount(text.Length);
                var memoryMappedInfo = this.CreateTemporaryStorage(size);

                // Write the source text out as Unicode. We expect that to be cheap.
                using var stream = memoryMappedInfo.CreateWritableStream();
                {
                    using var writer = new StreamWriter(stream, Encoding.Unicode);
                    text.Write(writer, cancellationToken);
                }

                return memoryMappedInfo;
            }
        }
    }

    public async Task<TemporaryStorageTextHandle> WriteToTemporaryStorageAsync(SourceText text, CancellationToken cancellationToken)
    {
        if (this._workspaceThreadingService is { IsOnMainThread: true })
        {
            await Task.Yield().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }

        return WriteToTemporaryStorage(text, cancellationToken);
    }

    ITemporaryStorageStreamHandle ITemporaryStorageServiceInternal.WriteToTemporaryStorage(Stream stream, CancellationToken cancellationToken)
        => WriteToTemporaryStorage(stream);

    public TemporaryStorageStreamHandle WriteToTemporaryStorage(Stream stream)
    {
        stream.Position = 0;
        var memoryMappedInfo = WriteToMemoryMappedFile();
        var identifier = new TemporaryStorageIdentifier(memoryMappedInfo.Name, memoryMappedInfo.Offset, memoryMappedInfo.Size);
        return new(memoryMappedInfo.MemoryMappedFile, identifier);

        MemoryMappedInfo WriteToMemoryMappedFile()
        {
            using (Logger.LogBlock(FunctionId.TemporaryStorageServiceFactory_WriteStream, CancellationToken.None))
            {
                var size = stream.Length;
                var memoryMappedInfo = this.CreateTemporaryStorage(size);
                using var viewStream = memoryMappedInfo.CreateWritableStream();
                {
                    using var pooledObject = SharedPools.ByteArray.GetPooledObject();
                    var buffer = pooledObject.Object;
                    while (true)
                    {
                        var count = stream.Read(buffer, 0, buffer.Length);
                        if (count == 0)
                            break;

                        viewStream.Write(buffer, 0, count);
                    }
                }

                return memoryMappedInfo;
            }
        }
    }

    internal static TemporaryStorageStreamHandle GetStreamHandle(TemporaryStorageIdentifier storageIdentifier)
    {
        Contract.ThrowIfFalse(PlatformInformation.IsWindows, $"{nameof(GetStreamHandle)} should only be called for VS on Windows (where named memory mapped files as supported)");
        Contract.ThrowIfNull(storageIdentifier.Name, $"{nameof(GetStreamHandle)} should only be called for VS on Windows (where named memory mapped files as supported)");
        var memoryMappedFile = MemoryMappedFile.OpenExisting(storageIdentifier.Name);
        return new(memoryMappedFile, storageIdentifier);
    }

    internal TemporaryStorageTextHandle GetTextHandle(
        TemporaryStorageIdentifier storageIdentifier,
        SourceHashAlgorithm checksumAlgorithm,
        Encoding? encoding,
        ImmutableArray<byte> contentHash)
    {
        Contract.ThrowIfNull(storageIdentifier.Name, $"{nameof(GetTextHandle)} should only be called for VS on Windows (where named memory mapped files as supported)");
        var memoryMappedFile = MemoryMappedFile.OpenExisting(storageIdentifier.Name);
        return new(this, memoryMappedFile, storageIdentifier, checksumAlgorithm, encoding, contentHash);
    }

    /// <summary>
    /// Allocate storage of a specified size in a new memory mapped file.
    /// </summary>
    /// <remarks>
    /// <para>Each allocation gets its own dedicated memory mapped file. This ensures that when a handle to the
    /// storage is released (and the <see cref="MemoryMappedFile"/> is finalized), the memory is freed immediately
    /// without being blocked by unrelated handles sharing the same file. A prior implementation used a bump-pointer
    /// allocator that packed small items into shared 8 MB blocks. While this reduced the number of memory mapped
    /// files, it caused a fragmentation problem: a single surviving handle into a shared block would keep the
    /// entire block alive. In long-running test hosts (especially x86), hundreds of blocks accumulated and
    /// exhausted virtual address space.</para>
    /// </remarks>
    /// <param name="size">The size of the storage block to allocate.</param>
    /// <returns>A <see cref="MemoryMappedInfo"/> describing the allocated block.</returns>
    private MemoryMappedInfo CreateTemporaryStorage(long size)
    {
        var name = CreateUniqueName(size);
        // MemoryMappedFile requires a capacity of at least 1 byte.
        var memoryMappedFile = MemoryMappedFile.CreateNew(name, Math.Max(size, 1));
        return new MemoryMappedInfo(memoryMappedFile, name, offset: 0, size: size);
    }

    public static string? CreateUniqueName(long size)
    {
        // MemoryMapped files which are used by the TemporaryStorageService are present in .NET Framework (including
        // Mono) and .NET Core Windows. For non-Windows .NET Core scenarios, we return null to enable create the memory
        // mapped file (just not in a way that can be shared across processes).
        return PlatformInformation.IsWindows || PlatformInformation.IsRunningOnMono
            ? $"Roslyn Shared File: Size={size} Id={Guid.NewGuid():N}"
            : null;
    }

    public sealed class TemporaryStorageTextHandle(
        TemporaryStorageService storageService,
        MemoryMappedFile memoryMappedFile,
        TemporaryStorageIdentifier identifier,
        SourceHashAlgorithm checksumAlgorithm,
        Encoding? encoding,
        ImmutableArray<byte> contentHash)
        : ITemporaryStorageTextHandle
    {
        public TemporaryStorageIdentifier Identifier => identifier;
        public SourceHashAlgorithm ChecksumAlgorithm => checksumAlgorithm;
        public Encoding? Encoding => encoding;
        public ImmutableArray<byte> ContentHash => contentHash;

        public async Task<SourceText> ReadFromTemporaryStorageAsync(CancellationToken cancellationToken)
        {
            // There is a reason for implementing it like this: proper async implementation
            // that reads the underlying memory mapped file stream in an asynchronous fashion
            // doesn't actually work. Windows doesn't offer
            // any non-blocking way to read from a memory mapped file; the underlying memcpy
            // may block as the memory pages back in and that's something you have to live
            // with. Therefore, any implementation that attempts to use async will still
            // always be blocking at least one threadpool thread in the memcpy in the case
            // of a page fault. Therefore, if we're going to be blocking a thread, we should
            // just block one thread and do the whole thing at once vs. a fake "async"
            // implementation which will continue to requeue work back to the thread pool.
            if (storageService._workspaceThreadingService is { IsOnMainThread: true })
            {
                await Task.Yield().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return ReadFromTemporaryStorage(cancellationToken);
        }

        public SourceText ReadFromTemporaryStorage(CancellationToken cancellationToken)
        {
            using (Logger.LogBlock(FunctionId.TemporaryStorageServiceFactory_ReadText, cancellationToken))
            {
                var info = new MemoryMappedInfo(memoryMappedFile, Identifier.Name, Identifier.Offset, Identifier.Size);
                using var stream = info.CreateReadableStream();
                using var reader = CreateTextReaderFromTemporaryStorage(stream);

                // we pass in encoding we got from original source text even if it is null.
                return storageService._textFactory.CreateText(reader, encoding, checksumAlgorithm, cancellationToken);
            }
        }

        private static unsafe DirectMemoryAccessStreamReader CreateTextReaderFromTemporaryStorage(UnmanagedMemoryStream stream)
        {
            var src = (char*)stream.PositionPointer;

            // BOM: Unicode, little endian
            // Skip the BOM when creating the reader
            Debug.Assert(*src == 0xFEFF);

            return new DirectMemoryAccessStreamReader(src + 1, (int)stream.Length / sizeof(char) - 1);
        }
    }
}
