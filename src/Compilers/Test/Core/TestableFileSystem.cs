// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Roslyn.Utilities;

namespace Roslyn.Test.Utilities
{
    public sealed class TestableFileSystem : ICompilerFileSystem
    {
        public delegate Stream NewFileStreamFunc(string filePath, FileMode mode, FileAccess access, FileShare share);
        public delegate Stream NewFileStreamExFunc(string filePath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, out string normalizedFilePath);
        public delegate bool FileExistsFunc(string filePath);
        public delegate byte[] FileReadAllBytesFunc(string filePath);

        private readonly Dictionary<string, TestableFile>? _map;
        private readonly FileExistsFunc _fileExists;
        private readonly FileReadAllBytesFunc _fileReadAllBytes;
        private readonly NewFileStreamFunc _newFileStream;
        private readonly NewFileStreamExFunc _newFileStreamEx;

        public Dictionary<string, TestableFile> Map => _map ?? throw new InvalidOperationException();
        public bool UsingMap => _map is not null;

        private TestableFileSystem(
            FileExistsFunc? fileExists = null,
            FileReadAllBytesFunc? fileReadAllBytes = null,
            NewFileStreamFunc? newFileStream = null,
            NewFileStreamExFunc? newFileStreamEx = null,
            Dictionary<string, TestableFile>? map = null)
        {
            _fileExists = fileExists ?? delegate { throw new InvalidOperationException(); };
            _fileReadAllBytes = fileReadAllBytes ?? delegate { throw new InvalidOperationException(); };
            _newFileStream = newFileStream ?? delegate { throw new InvalidOperationException(); };
            _newFileStreamEx = newFileStreamEx ?? (Stream (string _, FileMode _, FileAccess _, FileShare _, int _, FileOptions _, out string _) => throw new InvalidOperationException());
            _map = map;
        }

        public bool FileExists(string filePath) => _fileExists(filePath);

        public byte[] FileReadAllBytes(string filePath) => _fileReadAllBytes(filePath);

        public Stream NewFileStream(string filePath, FileMode mode, FileAccess access, FileShare share)
            => _newFileStream(filePath, mode, access, share);

        public Stream NewFileStreamEx(string filePath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, out string normalizedFilePath)
            => _newFileStreamEx(filePath, mode, access, share, bufferSize, options, out normalizedFilePath);

        public static TestableFileSystem CreateForStandard(
            FileExistsFunc? fileExists = null,
            FileReadAllBytesFunc? fileReadAllBytes = null,
            NewFileStreamFunc? newFileStream = null,
            NewFileStreamExFunc? newFileStreamEx = null)
            => new TestableFileSystem(
                fileExists: fileExists ?? StandardFileSystem.Instance.FileExists,
                fileReadAllBytes: fileReadAllBytes ?? StandardFileSystem.Instance.FileReadAllBytes,
                newFileStream: newFileStream ?? StandardFileSystem.Instance.NewFileStream,
                newFileStreamEx: newFileStreamEx ?? StandardFileSystem.Instance.NewFileStreamEx);

        public static TestableFileSystem CreateForOpenFile(NewFileStreamFunc newFileStream)
            => new TestableFileSystem(newFileStream: newFileStream);

        public static TestableFileSystem CreateForExistingPaths(IEnumerable<string> existingPaths, StringComparer? comparer = null)
        {
            comparer ??= StringComparer.OrdinalIgnoreCase;
            var set = new HashSet<string>(existingPaths, comparer);
            return new TestableFileSystem(fileExists: set.Contains);
        }

        public static TestableFileSystem CreateForFiles(params (string FilePath, TestableFile TestableFile)[] files)
        {
            var map = files.ToDictionary(
                x => x.FilePath,
                x => x.TestableFile);
            return CreateForMap(map);
        }

        public static TestableFileSystem CreateForMap() => CreateForMap(new());

        public static TestableFileSystem CreateForMap(Dictionary<string, TestableFile> map)
        {
            NewFileStreamExFunc newFileStreamEx = (string filePath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, out string normalizedFilePath) =>
            {
                normalizedFilePath = filePath;
                return map[filePath].GetStream(access);
            };

            return new TestableFileSystem(
                fileExists: map.ContainsKey,
                fileReadAllBytes: filePath => map[filePath].GetStream().ReadAllBytes(),
                newFileStream: Stream (string filePath, FileMode mode, FileAccess access, FileShare share) => map[filePath].GetStream(access),
                newFileStreamEx: newFileStreamEx,
                map: map);
        }
    }
}
