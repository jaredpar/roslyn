// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Cci;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests;

public sealed class PooledBlobBuilderTests
{
    [Theory]
    [InlineData(PooledBlobBuilder.PoolChunkSize / 2)]
    [InlineData(PooledBlobBuilder.PoolChunkSize)]
    [InlineData(PooledBlobBuilder.PoolChunkSize * 3)]
    [InlineData((PooledBlobBuilder.PoolChunkSize * 3) + 42)]
    public void TryWriteBytesSegmentedExact(int size)
    {
        var bytes = new byte[size];
        bytes.AsSpan().Fill(13);
        var builder = PooledBlobBuilder.GetInstance();
        var written = builder.TryWriteBytesSegmented(new MemoryStream(bytes), size);
        Assert.Equal(size, written);

        var expectedBlobCount = size % PooledBlobBuilder.PoolChunkSize == 0
            ? size / PooledBlobBuilder.PoolChunkSize
            : (size / PooledBlobBuilder.PoolChunkSize) + 1;

        var actualBlobCount = 0;
        foreach (var blob in builder.GetBlobs())
        {
            actualBlobCount++;
            foreach (var item in blob.GetBytes())
            {
                Assert.Equal(13, item);
            }
        }

        Assert.Equal(expectedBlobCount, actualBlobCount);
        builder.Free();
    }
}
