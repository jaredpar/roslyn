// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// This is an abstraction over the file system which allows for us to do more thorough unit testing.
    /// </summary>
    internal class StrongNameFileSystem
    {
        internal static readonly StrongNameFileSystem Instance = new StrongNameFileSystem(fileSystem: StandardFileSystem.Instance);
        private readonly string? _signingTempPath;
        private readonly ICompilerFileSystem _fileSystem;

        internal ICompilerFileSystem CompilerFileSystem => _fileSystem;

        internal StrongNameFileSystem(string? signingTempPath = null, ICompilerFileSystem? fileSystem = null)
        {
            _signingTempPath = signingTempPath;
            _fileSystem = fileSystem ?? StandardFileSystem.Instance;
        }

        internal virtual Stream CreateFileStream(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            return _fileSystem.NewFileStream(filePath, fileMode, fileAccess, fileShare);
        }

        internal virtual byte[] ReadAllBytes(string fullPath)
        {
            Debug.Assert(PathUtilities.IsAbsolute(fullPath));
            return _fileSystem.FileReadAllBytes(fullPath);
        }

        internal virtual bool FileExists(string? fullPath)
        {
            Debug.Assert(fullPath == null || PathUtilities.IsAbsolute(fullPath));
            return File.Exists(fullPath);
        }

        internal string? GetSigningTempPath() => _signingTempPath;
    }
}
