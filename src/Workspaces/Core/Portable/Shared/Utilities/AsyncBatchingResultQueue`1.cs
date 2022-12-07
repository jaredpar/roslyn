﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.Shared.TestHooks;

namespace Roslyn.Utilities
{
    /// <inheritdoc cref="AsyncBatchingWorkQueue{TItem, TResult}"/>
    internal class AsyncBatchingResultQueue<TResult> : AsyncBatchingWorkQueue<VoidResult, TResult>
    {
        public AsyncBatchingResultQueue(
            TimeSpan delay,
            Func<CancellationToken, ValueTask<TResult>> processBatchAsync,
            IAsynchronousOperationListener asyncListener,
            CancellationToken cancellationToken)
            : base(delay,
                   Convert(processBatchAsync),
                    EqualityComparer<VoidResult>.Default,
                   asyncListener,
                   cancellationToken)
        {
        }

        private static Func<ImmutableSegmentedList<VoidResult>, CancellationToken, ValueTask<TResult>> Convert(Func<CancellationToken, ValueTask<TResult>> processBatchAsync)
            => (items, ct) => processBatchAsync(ct);

        public void AddWork(bool cancelExistingWork = false)
            => base.AddWork(default(VoidResult), cancelExistingWork);
    }
}
