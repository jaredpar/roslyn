// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Options;

namespace RunTests
{
    internal class Options
    {
        public string Configuration { get; set; }

        /// <summary>
        /// The set of target frameworks that should be probed for test assemblies.
        /// </summary>
        public TestRuntime TestRuntime { get; set; } = TestRuntime.Both;

        public List<string> IncludeFilter { get; set; } = new List<string>();

        public List<string> ExcludeFilter { get; set; } = new List<string>();

        public string ArtifactsDirectory { get; }

        /// <summary>
        /// Name of the Helix queue to run tests on.
        /// </summary>
        public string? HelixQueueName { get; set; }

        /// <summary>
        /// Access token to send jobs to helix.
        /// This should only be set when using internal helix queues.
        /// </summary>
        public string? HelixApiAccessToken { get; set; }

        /// <summary>
        /// Path to the dotnet executable we should use for running dotnet test
        /// </summary>
        public string DotnetFilePath { get; set; }

        public string Architecture { get; set; }

        public string? AccessToken { get; set; }

        public string? ProjectUri { get; set; }

        public string? PipelineDefinitionId { get; set; }

        public string? PhaseName { get; set; }

        public string? TargetBranchName { get; set; }

        public Options(
            string dotnetFilePath,
            string artifactsDirectory,
            string configuration,
            string architecture)
        {
            DotnetFilePath = dotnetFilePath;
            ArtifactsDirectory = artifactsDirectory;
            Configuration = configuration;
            Architecture = architecture;
        }

        internal static Options? Parse(string[] args)
        {
            string? dotnetFilePath = null;
            var architecture = Microsoft.CodeAnalysis.Test.Utilities.IlasmUtilities.Architecture;
            var testRuntime = TestRuntime.Both;
            var configuration = "Debug";
            var includeFilter = new List<string>();
            var excludeFilter = new List<string>();
            var helixQueueName = "Windows.10.Amd64.Open";
            string? helixApiAccessToken = null;
            string? artifactsPath = null;
            string? accessToken = null;
            string? projectUri = null;
            string? pipelineDefinitionId = null;
            string? phaseName = null;
            string? targetBranchName = null;
            var optionSet = new OptionSet()
            {
                { "dotnet=", "Path to dotnet", s => dotnetFilePath = s },
                { "configuration=", "Configuration to test: Debug or Release", s => configuration = s },
                { "runtime=", "The runtime to test: both, core or framework", (TestRuntime t) => testRuntime = t},
                { "include=", "Expression for including unit test dlls: default *.UnitTests.dll", s => includeFilter.Add(s) },
                { "exclude=", "Expression for excluding unit test dlls: default is empty", s => excludeFilter.Add(s) },
                { "arch=", "Architecture to test on: x86, x64 or arm64", s => architecture = s },
                { "helixQueueName=", "Name of the Helix queue to run tests on", s => helixQueueName = s },
                { "helixApiAccessToken=", "Access token for internal helix queues", s => helixApiAccessToken = s },
                { "logs=", "Log file directory", s => { /* accepted for compatibility but unused */ } },
                { "artifactspath=", "Path to the artifacts directory", s => artifactsPath = s },
                { "accessToken=", "Pipeline access token with permissions to view test history", s => accessToken = s },
                { "projectUri=", "ADO project containing the pipeline", s => projectUri = s },
                { "pipelineDefinitionId=", "Pipeline definition id", s => pipelineDefinitionId = s },
                { "phaseName=", "Pipeline phase name associated with this test run", s => phaseName = s },
                { "targetBranchName=", "Target branch of this pipeline run", s => targetBranchName = s },
            };

            try
            {
                optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                ConsoleUtil.WriteLine($"Error parsing command line arguments: {e.Message}");
                optionSet.WriteOptionDescriptions(Console.Out);
                return null;
            }

            if (includeFilter.Count == 0)
            {
                includeFilter.Add(".*UnitTests.*");
            }

            artifactsPath ??= TryGetArtifactsPath();
            if (artifactsPath is null || !Directory.Exists(artifactsPath))
            {
                ConsoleUtil.WriteLine($"Did not find artifacts directory at {artifactsPath}");
                return null;
            }

            dotnetFilePath ??= TryGetDotNetPath();
            if (dotnetFilePath is null || !File.Exists(dotnetFilePath))
            {
                ConsoleUtil.WriteLine($"Did not find 'dotnet' at {dotnetFilePath}");
                return null;
            }

            return new Options(
                dotnetFilePath: dotnetFilePath,
                artifactsDirectory: artifactsPath,
                configuration: configuration,
                architecture: architecture)
            {
                TestRuntime = testRuntime,
                IncludeFilter = includeFilter,
                ExcludeFilter = excludeFilter,
                HelixQueueName = helixQueueName,
                HelixApiAccessToken = helixApiAccessToken,
                AccessToken = accessToken,
                ProjectUri = projectUri,
                PipelineDefinitionId = pipelineDefinitionId,
                PhaseName = phaseName,
                TargetBranchName = targetBranchName,
            };

            static string? TryGetArtifactsPath()
            {
                var path = AppContext.BaseDirectory;
                while (path is object && Path.GetFileName(path) != "artifacts")
                {
                    path = Path.GetDirectoryName(path);
                }

                return path;
            }

            static string? TryGetDotNetPath()
            {
                var dir = RuntimeEnvironment.GetRuntimeDirectory();
                var programName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";

                while (dir != null && !File.Exists(Path.Combine(dir, programName)))
                {
                    dir = Path.GetDirectoryName(dir);
                }

                return dir == null ? null : Path.Combine(dir, programName);
            }
        }
    }
}
