// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    internal abstract partial class CommonCompiler
    {
        internal readonly struct EmitPaths
        {
            internal string PeFilePath { get; }
            internal string RefPeFilePath { get; }
            internal string PdbFilePath { get; }
            internal string XmlFilePath { get; }

            internal EmitPaths(string outputName, CommandLineArguments arguments)
            {
                PeFilePath = Path.Combine(arguments.OutputDirectory, outputName);
                RefPeFilePath = arguments.OutputRefFilePath;
                PdbFilePath = arguments.PdbPath ?? Path.ChangeExtension(PeFilePath, ".pdb");
                XmlFilePath = arguments.DocumentationPath;
            }
        }

    }
}
