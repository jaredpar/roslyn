// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;

namespace Roslyn.Utilities
{
    /// <summary>
    /// Abstraction over the file system that is useful for test hooks and tracking access 
    /// to the file system for build reporting.
    /// </summary>
    internal interface ICompilerFileSystem
    {
#pragma warning disable RS0030 // Using APIs in doc comments for reference

        /// <summary>
        /// <see cref="System.IO.File.Exists(string?)"/>
        /// </summary>
        bool FileExists(string filePath);

        /// <summary>
        /// <see cref="File.ReadAllBytes(string)"/>
        /// </summary>
        byte[] FileReadAllBytes(string filePath);

        /// <summary>
        /// <see cref="FileStream.FileStream(string, FileMode, FileAccess, FileShare)"/>
        /// </summary>
        Stream NewFileStream(string filePath, FileMode mode, FileAccess access, FileShare share);

        /// <summary>
        /// <see cref="FileStream.FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)"/> but includes
        /// the normalization of the path used to open the file.
        /// </summary>
        Stream NewFileStreamEx(string filePath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, out string normalizedFilePath);

#pragma warning restore RS0030
    }

    internal static class CompilerFileSystemExtensions
    {
        /// <summary>
        /// Open a file and ensure common exception types are wrapped to <see cref="IOException"/>.
        /// </summary>
        internal static Stream OpenFileWithNormalizedException(this ICompilerFileSystem fileSystem, string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            try
            {
                return fileSystem.NewFileStream(filePath, fileMode, fileAccess, fileShare);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (DirectoryNotFoundException e)
            {
                throw new FileNotFoundException(e.Message, filePath, e);
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new IOException(e.Message, e);
            }
        }

        /// <summary>
        /// <see cref="FileStream.FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)"/>
        /// </summary>
        internal static Stream NewFileStream(this ICompilerFileSystem fileSystem, string filePath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) =>
            fileSystem.NewFileStream(filePath, mode, access, share, bufferSize, options);
    }

    // Disable as this is the class that is specifically allowed to access the file system
#pragma warning disable RS0030 

    internal sealed class StandardFileSystem : ICompilerFileSystem
    {
        public static StandardFileSystem Instance { get; } = new StandardFileSystem();

        private StandardFileSystem()
        {
        }

        public bool FileExists(string filePath) => File.Exists(filePath);

        public byte[] FileReadAllBytes(string filePath) => File.ReadAllBytes(filePath);

        public Stream NewFileStream(string filePath, FileMode mode, FileAccess access, FileShare share)
            => new FileStream(filePath, mode, access, share);

        public Stream NewFileStreamEx(string filePath, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, out string normalizedFilePath)
        {
            var fileStream = new FileStream(filePath, mode, access, share, bufferSize, options);
            normalizedFilePath = fileStream.Name;
            return fileStream;
        }
    }

#pragma warning restore RS0030
}
