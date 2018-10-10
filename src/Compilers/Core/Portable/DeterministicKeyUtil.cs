// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.CodeAnalysis
{
    internal abstract class DeterministicKeyUtil
    {
        internal static void EmitDeterminismKey(CommandLineArguments args, string[] rawArgs, string baseDirectory, CommandLineParser parser)
        {
            var key = CreateDeterminismKey(args, rawArgs, baseDirectory, parser);
            var filePath = Path.Combine(args.OutputDirectory, args.OutputFileName + ".key");
            using (var stream = File.Create(filePath))
            {
                var bytes = Encoding.UTF8.GetBytes(key);
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        /// <summary>
        /// The string returned from this function represents the inputs to the compiler which impact determinism.  It is 
        /// meant to be inline with the specification here:
        /// 
        ///     - https://github.com/dotnet/roslyn/blob/master/docs/compilers/Deterministic%20Inputs.md
        /// 
        /// Issue #8193 tracks filling this out to the full specification. 
        /// 
        ///     https://github.com/dotnet/roslyn/issues/8193
        /// </summary>
        internal static string CreateDeterminismKey(CommandLineArguments args, string[] rawArgs, string baseDirectory, CommandLineParser parser)
        {
            List<Diagnostic> diagnostics = new List<Diagnostic>();
            List<string> flattenedArgs = new List<string>();
            parser.FlattenArgs(rawArgs, diagnostics, flattenedArgs, null, baseDirectory);

            var builder = new StringBuilder();
            var name = !string.IsNullOrEmpty(args.OutputFileName)
                ? Path.GetFileNameWithoutExtension(Path.GetFileName(args.OutputFileName))
                : $"no-output-name-{Guid.NewGuid().ToString()}";

            builder.AppendLine($"{name}");
            builder.AppendLine($"Command Line:");
            foreach (var current in flattenedArgs)
            {
                builder.AppendLine($"\t{current}");
            }

            builder.AppendLine("Source Files:");
            var hash = MD5.Create();
            foreach (var sourceFile in args.SourceFiles)
            {
                var sourceFileName = Path.GetFileName(sourceFile.Path);

                string hashValue;
                try
                {
                    var bytes = File.ReadAllBytes(sourceFile.Path);
                    var hashBytes = hash.ComputeHash(bytes);
                    var data = BitConverter.ToString(hashBytes);
                    hashValue = data.Replace("-", "");
                }
                catch (Exception ex)
                {
                    hashValue = $"Could not compute {ex.Message}";
                }
                builder.AppendLine($"\t{sourceFileName} - {hashValue}");
            }

            return builder.ToString();
        }
    }
}
