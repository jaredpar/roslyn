﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RunTests
{
    internal sealed class Program
    {
        internal static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage();
                return 1;
            }

            var xunitPath = args[0];
            var index = 1;
            var test64 = false;
            var testParallel = false;
            var useHtml = true;
            ParseArgs(args, ref index, ref test64, ref testParallel, ref useHtml);
            Console.WriteLine("{0} {1} {2} {3}", index, test64, testParallel, useHtml);

            var list = new List<string>(args.Skip(index));
            if (list.Count == 0)
            {
                PrintUsage();
                return 1;
            }

            var xunit = test64
                ? Path.Combine(xunitPath, "xunit.console.exe")
                : Path.Combine(xunitPath, "xunit.console.x86.exe");

            // Setup cancellation for ctrl-c key presses
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += delegate
            {
                cts.Cancel();
            };

            var testRunner = new TestRunner(xunit, useHtml, testParallel);
            var start = DateTime.Now;
            Console.WriteLine("Running {0} tests", list.Count);
            var result = testRunner.RunAllAsync(list, cts.Token).Result;
            var span = DateTime.Now - start;
            if (!result)
            {
                ConsoleUtil.WriteLine(ConsoleColor.Red, "Test failures encountered: {0}", span);
                return 1;
            }

            Console.WriteLine("All tests passed: {0}", span);
            return 0;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("runtests [xunit-console-runner] [assembly1] [assembly2] [...]");
        }

        private static void ParseArgs(string[] args, ref int index, ref bool test64, ref bool testParallel, ref bool useHtml)
        {
            var comp = StringComparer.OrdinalIgnoreCase;
            while (index < args.Length)
            {
                var current = args[index];
                Console.WriteLine("considering '{0}'", current);
                if (comp.Equals(current, "-test64"))
                {
                    test64 = true;
                    index++;
                }
                else if (comp.Equals(current, "-testparallel"))
                {
                    testParallel = true;
                    index++;
                }
                else if (comp.Equals(current, "-xml"))
                {
                    useHtml = false;
                    index++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
