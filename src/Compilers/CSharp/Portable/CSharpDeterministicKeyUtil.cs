// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.CSharp
{
    // PROTOTYPE: do we need emit options here?
    internal sealed class CSharpDeterministicKeyUtil : DeterministicKeyUtil
    {
        private static void AppendCompilationOptions(StringBuilder builder, CSharpCompilationOptions options)
        {
            // PROTOTYPE: usings
            AppendCommonCompilationOptions(builder, options);
            builder.AppendLine($"unsafe: {options.AllowUnsafe}");
        }

        internal static string GenerateKey(CSharpCompilation compilation)
        {
            var builder = new StringBuilder();
            AppendCompilationOptions(builder, compilation.Options);
            AppendSyntaxTrees(builder, compilation.SyntaxTrees);
            AppendReferences(builder, compilation.References);
            return builder.ToString();
        }
    }
}
