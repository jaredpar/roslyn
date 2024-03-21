// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis;

internal static class BlobBuilderExtensions
{
    internal static unsafe void WriteBytes(this BlobBuilder builder, ReadOnlySpan<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return;
        }

        fixed (byte* bytes = buffer)
        {
            builder.WriteBytes(bytes, buffer.Length);
        }
    }
}
