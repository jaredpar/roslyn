// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RunTests
{
    internal sealed class Program
    {
        internal const int ExitSuccess = 0;
        internal const int ExitFailure = 1;

        internal static async Task<int> Main(string[] args)
        {
            Logger.Log("RunHelix command line");
            Logger.Log(string.Join(" ", args));
            var options = Options.Parse(args);
            if (options == null)
            {
                return ExitFailure;
            }

            ConsoleUtil.WriteLine($"Running '{options.DotnetFilePath} --version'..");
            var dotnetResult = await ProcessRunner.CreateProcess(options.DotnetFilePath, arguments: "--version", captureOutput: true).Result;
            ConsoleUtil.WriteLine(string.Join(Environment.NewLine, dotnetResult.OutputLines));
            ConsoleUtil.WriteLine(ConsoleColor.Red, string.Join(Environment.NewLine, dotnetResult.ErrorLines));

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += delegate
            {
                cts.Cancel();
            };

            var assemblyFilePaths = AssemblyDiscovery.GetAssemblyFilePaths(
                options.ArtifactsDirectory,
                options.Configuration,
                options.TestRuntime,
                options.IncludeFilter,
                options.ExcludeFilter);
            return await HelixTestRunner.RunAsync(
                options,
                assemblyFilePaths,
                cts.Token);
        }
    }
}
