// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.UnitTests
{
    public partial class AsyncLazyTests
    {
        /// <summary>
        /// For some of the AsyncLazy tests, we want to see what happens if the thread pool is
        /// really behind on processing tasks. This code blocks up the thread pool with pointless work to 
        /// ensure nothing ever runs.
        /// </summary>
        private sealed class StopTheThreadPoolContext : IDisposable
        {
            private static readonly Mutex s_guard = new Mutex(initiallyOwned: false);
            private readonly ManualResetEventSlim _testComplete = new ManualResetEventSlim();

            public StopTheThreadPoolContext()
            {
                s_guard.WaitOne();

                int workerThreads;
                int ioThreads;
                ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
                var barrier = new Barrier(workerThreads + 1);
                for (int i = 0; i < workerThreads; i++)
                {
                    ThreadPool.QueueUserWorkItem(
                        delegate
                        {
                            barrier.SignalAndWait();
                            _testComplete.Wait();
                        });
                }

                barrier.SignalAndWait();
            }

            public void Dispose()
            {
                _testComplete.Set();
                s_guard.ReleaseMutex();
            }
        }
    }
}
