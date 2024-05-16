// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.IL.Transforms;
using Newtonsoft.Json;

namespace RunTests;

/// <summary>
/// This type is responsible for generating the build files for executing tests on Helix
/// </summary>
internal static class HelixUtil
{
    internal static void GenerateHelixArtifacts(ImmutableArray<WorkItemInfo> workItems, Options options) =>
        GenerateHelixArtifacts(
            options.HelixJobName,
            options.HelixQueueName,
            options.Architecture,
            options.HelixOSPlatform,
            options.ArtifactsDirectory,
            workItems);

    /// <summary>
    /// Generate the helix artifacts for the given set of work items
    /// </summary>
    /// <remarks>
    /// Keep in mind tha the artifacts layout looks like the following:
    /// 
    ///   - artifacts/testPayloads
    ///     - .duplicate
    ///     - .helix/{helix-name}
    ///         - helix.csproj
    ///         - workitems
    ///             - {partitionIndex} 
    ///                 - vstest.rsp
    ///                 - copy-dumps.sh
    ///                 - rehydrate.sh
    ///                 - runtests.sh
    /// This relative path structure is important for how the msbuild files are generated
    /// </remarks>
    internal static void GenerateHelixArtifacts(
        string? helixJobName,
        string? helixQueueName,
        string architecture,
        OSPlatform platform,
        string artifactsDirectory,
        ImmutableArray<WorkItemInfo> workItems)
    {
        if (string.IsNullOrEmpty(helixJobName))
        {
            throw new Exception($"{nameof(helixJobName)} must be set to generate Helix artifacts");
        }

        if (string.IsNullOrEmpty(helixQueueName))
        {
            throw new Exception($"{nameof(helixQueueName)} must be set to generate Helix artifacts");
        }

        var sourceBranch = Environment.GetEnvironmentVariable("BUILD_SOURCEBRANCH");
        if (sourceBranch is null)
        {
            sourceBranch = "local";
            ConsoleUtil.Warning($@"BUILD_SOURCEBRANCH environment variable was not set. Using source branch ""{sourceBranch}"" instead");
        }

        // https://github.com/dotnet/roslyn/issues/50661
        // it's possible we should be using the BUILD_SOURCEVERSIONAUTHOR instead here a la https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Helix/Sdk/tools/xharness-runner/Readme.md#how-to-use
        // however that variable isn't documented at https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml
        var queuedBy = Environment.GetEnvironmentVariable("BUILD_QUEUEDBY")?.Replace(" ", "");
        if (queuedBy is null)
        {
            queuedBy = "roslyn";
            ConsoleUtil.Warning($@"BUILD_QUEUEDBY environment variable was not set. Using value ""{queuedBy}"" instead");
        }

        var jobName = Environment.GetEnvironmentVariable("SYSTEM_JOBDISPLAYNAME");
        if (jobName is null)
        {
            ConsoleUtil.Warning($"SYSTEM_JOBDISPLAYNAME environment variable was not set. Using a blank TestRunNamePrefix for Helix job.");
        }

        var buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");
        if (buildNumber is null)
        {
            buildNumber = "0";
            ConsoleUtil.Warning($"BUILD_BUILDNUMBER environment variable was not set. Using 0");
        }

        var helixDirectory = Path.Combine(artifactsDirectory, "testPayloads", ".helix", helixJobName);
        var workItemsDirectory = Path.Combine(helixDirectory, "workitems");
        _ = Directory.CreateDirectory(workItemsDirectory);

        var dotnetSdkVersion = GetDotNetSdkVersion(artifactsDirectory);
        var project = $"""
            <Project Sdk="Microsoft.DotNet.Helix.Sdk" DefaultTargets="Test">
                <PropertyGroup>
                    <TestRunNamePrefix>{jobName}_</TestRunNamePrefix>
                    <HelixSource>pr/{sourceBranch}</HelixSource>
                    <HelixType>test</HelixType>
                    <HelixBuild>{buildNumber}/HelixBuild>
                    <HelixTargetQueues>{helixQueueName}</HelixTargetQueues>
                    <IncludeDotNetCli>true</IncludeDotNetCli>
                    <DotNetCliVersion>{dotnetSdkVersion}</DotNetCliVersion>
                    <DotNetCliPackageType>sdk</DotNetCliPackageType>
                    <BUILD_REPOSITORY_NAME>dotnet/roslyn</BUILD_REPOSITORY_NAME>
                    <BUILD_REASON>pr</BUILD_REASON>
                    <SYSTEM_TEAMPROJECT>dnceng</SYSTEM_TEAMPROJECT>
                    <EnableAzurePipelinesReporter>true</EnableAzurePipelinesReporter>
                </PropertyGroup>

                <ItemGroup>
                    <HelixCorrelationPayload Include="../../.duplicate" />

            """;

        foreach (var workItemInfo in workItems)
        {
            var content = MakeHelixWorkItemProject(
                helixJobName,
                artifactsDirectory,
                workItemsDirectory,
                workItemInfo,
                platform,
                architecture);
            project += content + Environment.NewLine;
        }

        project += """
                </ItemGroup>
            </Project>

            """;

        File.WriteAllText(Path.Combine(helixDirectory, "helix.csproj"), project);

        static string GetDotNetSdkVersion(string artifactsDirectory)
        {
            var globalJsonFilePath = GetGlobalJsonFilePath(artifactsDirectory);
            var text = File.ReadAllText(globalJsonFilePath);
            var globalJson = JsonConvert.DeserializeAnonymousType(text, new { sdk = new { version = "" } })!;
            var version = globalJson.sdk.version;
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception($"Could not read sdk version from {globalJsonFilePath}.");
            }

            return version;
        }

        static string GetGlobalJsonFilePath(string artifactsDirectory)
        {
            var path = artifactsDirectory;
            while (path is object)
            {
                var globalJsonPath = Path.Join(path, "global.json");
                if (File.Exists(globalJsonPath))
                {
                    return globalJsonPath;
                }

                path = Path.GetDirectoryName(path);
            }
            throw new Exception($@"Could not find global.json by walking up from ""{artifactsDirectory}"".");
        }

        static string MakeHelixWorkItemProject(
            string helixJobName,
            string artifactsDirectory,
            string workItemsDirectory,
            WorkItemInfo workItemInfo,
            OSPlatform platform,
            string architecture)
        {
            // TODO: is this still true?
            // Currently, it's required for the client machine to use the same OS family as the target Helix queue.
            // We could relax this and allow for example Linux clients to kick off Windows jobs, but we'd have to
            // figure out solutions for issues such as creating file paths in the correct format for the target machine.
            var isUnixLike = platform != OSPlatform.Windows;
            var isMac = platform == OSPlatform.OSX;

            // Update the assembly groups to test with the assembly paths in the context of the helix work item.
            workItemInfo = workItemInfo with { Filters = workItemInfo.Filters.ToImmutableSortedDictionary(kvp => kvp.Key with { AssemblyPath = GetAssemblyPathInHelix(kvp.Key.AssemblyPath) }, kvp => kvp.Value) };
            var payloadDirectory = Path.Combine(workItemsDirectory, workItemInfo.PartitionIndex.ToString());
            _ = Directory.CreateDirectory(payloadDirectory);

            var preCommands = SecurityElement.Escape(GetPreCommandsContent());
            var command = SecurityElement.Escape(GetCommandContent());
            var postCommands = SecurityElement.Escape(GetPostCommandsContent());
            return $"""
                        <HelixWorkItem Include="{workItemInfo.DisplayName}">
                            <PayloadDirectory>{payloadDirectory}</PayloadDirectory>
                            <PreCommands>{preCommands}</PreCommands>
                            <Command>{command}<Comand>
                            <PostCommands>{postCommands}</PostCommands>
                            <Timeout>00:30:00</Timeout>
                        </HelixWorkItem>
                """;

            string GetAsEnvironmentVariable(string name) =>
                isUnixLike ? "${name}" : "%name%";

            string GetPreCommandsContent()
            {
                var builder = new StringBuilder();

                // Rehydrate assemblies that we need to run as part of this work item.
                foreach (var testAssembly in workItemInfo.Filters.Keys)
                {
                    var directoryName = Path.GetDirectoryName(testAssembly.AssemblyPath);
                    if (isUnixLike)
                    {
                        // If we're on unix make sure we have permissions to run the rehydrate script.
                        builder.AppendLine($"chmod +x {directoryName}/rehydrate.sh");
                    }

                    builder.AppendLine(isUnixLike ? $"./{directoryName}/rehydrate.sh" : $@"call {directoryName}\rehydrate.cmd");
                    builder.AppendLine(isUnixLike ? $"ls -l {directoryName}" : $"dir {directoryName}");
                }

                return builder.ToString();
            }

            string GetCommandContent()
            {
                // First write out all of the necessary environment variables
                var setEnvironmentVariable = isUnixLike ? "export" : "set";
                var command = new StringBuilder();
                command.AppendLine($"{setEnvironmentVariable} DOTNET_ROLL_FORWARD=LatestMajor");
                command.AppendLine($"{setEnvironmentVariable} DOTNET_ROLL_FORWARD_TO_PRERELEASE=1");

                // OSX produces extremely large dump files that commonly exceed the limits of Helix 
                // uploads. These settings limit the dump file size + produce a .json detailing crash 
                // reasons that work better with Helix size limitations.
                if (isMac)
                {
                    command.AppendLine($"{setEnvironmentVariable} DOTNET_DbgEnableMiniDump=1");
                    command.AppendLine($"{setEnvironmentVariable} DOTNET_DbgMiniDumpType=1");
                    command.AppendLine($"{setEnvironmentVariable} DOTNET_EnableCrashReport=1");
                }

                // Set the dump folder so that dotnet writes all dump files to this location automatically. 
                // This saves the need to scan for all the different types of dump files later and copy
                // them around.
                var helixDumpFolder = $"{GetAsEnvironmentVariable("HELIX_DUMP_FOLDER")}crash.%d.%e.dmp";
                command.AppendLine($"{setEnvironmentVariable} DOTNET_DbgMiniDumpName=\"{helixDumpFolder}\"");

                string[] knownEnvironmentVariables =
                [
                    "ROSLYN_TEST_IOPERATION",
                    "ROSLYN_TEST_USEDASSEMBLIES"
                ];

                foreach (var knownEnvironmentVariable in knownEnvironmentVariables)
                {
                    if (Environment.GetEnvironmentVariable(knownEnvironmentVariable) is string { Length: > 0 } value)
                    {
                        command.AppendLine($"{setEnvironmentVariable} {knownEnvironmentVariable}=\"{value}\"");
                    }
                }

                // Next run a few commands that dump environment info that is useful for debugging helix issues
                command.AppendLine(isUnixLike ? $"ls -l" : $"dir");
                command.AppendLine(isUnixLike ? "env | sort" : "set");
                command.AppendLine("dotnet --info");

                // Build an rsp file to send to dotnet test that contains all the assemblies and tests to run.
                // This gets around command line length limitations and avoids weird escaping issues.
                // See https://docs.microsoft.com/en-us/dotnet/standard/commandline/syntax#response-files
                var rspFileName = "vstest.rsp";
                var rspRelativeFileName = Path.Combine(workItemInfo.PartitionIndex.ToString(), rspFileName);
                File.WriteAllText(
                    Path.Combine(payloadDirectory, rspFileName),
                    GetTestRspFileContents(helixJobName, workItemInfo, architecture));

                // Build the command to run the rsp file.
                // dotnet test does not pass rsp files correctly the vs test console, so we have to manually invoke vs test console.
                // See https://github.com/microsoft/vstest/issues/3513
                // The dotnet sdk includes the vstest.console.dll executable in the sdk folder in the installed version, so we look it up using the
                // DOTNET_ROOT environment variable set by helix.
                if (isUnixLike)
                {
                    // $ is a special character in msbuild so we replace it with %24 in the helix project.
                    // https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-special-characters?view=vs-2022
                    command.AppendLine("vstestConsolePath=%24(find %24{DOTNET_ROOT} -name \"vstest.console.dll\")");
                    command.AppendLine("echo %24{vstestConsolePath}");
                    command.AppendLine($"dotnet exec \"%24{{vstestConsolePath}}\" @{rspRelativeFileName}");
                }
                else
                {
                    // Windows cmd doesn't have an easy way to set the output of a command to a variable.
                    // So send the output of the command to a file, then set the variable based on the file.
                    command.AppendLine("where /r %DOTNET_ROOT% vstest.console.dll > temp.txt");
                    command.AppendLine("set /p vstestConsolePath=<temp.txt");
                    command.AppendLine("echo %vstestConsolePath%");
                    command.AppendLine($"dotnet exec \"%vstestConsolePath%\" @{rspRelativeFileName}");
                }

                return command.ToString();
            }

            string GetPostCommandsContent()
            {
                // We want to collect any dumps during the post command step here; these commands are ran after the
                // return value of the main command is captured; a Helix Job is considered to fail if the main command returns a
                // non-zero error code, and we don't want the cleanup steps to interefere with that. PostCommands exist
                // precisely to address this problem.
                //
                // This is still necessary even with us setting  DOTNET_DbgMiniDumpName because the system can create 
                // non .NET Core dump files that aren't controlled by that value.
                var command = new StringBuilder();

                if (isUnixLike)
                {
                    // Write out this command into a separate file; unfortunately the use of single quotes and ; that is required
                    // for the command to work causes too much escaping issues in MSBuild.
                    File.WriteAllText(Path.Combine(payloadDirectory, "copy-dumps.sh"), "find . -name '*.dmp' -exec cp {} $HELIX_DUMP_FOLDER \\;");
                    command.AppendLine("./copy-dumps.sh");
                }
                else
                {
                    command.AppendLine("for /r %%f in (*.dmp) do copy %%f %HELIX_DUMP_FOLDER%");
                }

                return command.ToString();
            }

            // The entire artifacts directory is copied to the helix correlation payload. Just need to 
            // remove parts of the full path and replace it with the helix environment variables.
            string GetAssemblyPathInHelix(string assemblyPath)
            {
                Debug.Assert(assemblyPath.StartsWith(artifactsDirectory, StringComparison.Ordinal));
                var dir = Path.GetDirectoryName(assemblyPath)!;
                var path = Path.GetRelativePath(artifactsDirectory, assemblyPath);
                return $"{GetAsEnvironmentVariable("HELIX_CORRELATION_PAYLOAD")}{path}";
            }

            static string GetTestRspFileContents(
                string helixJobName,
                WorkItemInfo workItemInfo,
                string architecture)
            {
                var builder = new StringBuilder();

                var xmlFilePath = $"WorkItem-{helixJobName}-{workItemInfo.PartitionIndex}-test-results.xml";

                // Add each assembly we want to test on a new line.
                var assemblyPaths = workItemInfo.Filters.Keys .Select(assembly => assembly.AssemblyPath);
                foreach (var path in assemblyPaths)
                {
                    builder.AppendLine($"\"{path}\"");
                }

                builder.AppendLine($@"/Platform:{architecture}");
                builder.AppendLine($@"/Logger:xunit;LogFilePath={xmlFilePath}");
                builder.AppendLine($"/Blame:CollectDumpCollectionHangDump;TestTimeout=15minutes;DumpType=full");
                builder.AppendLine($"/ResultsDirectory:.");

                if (workItemInfo.Filters.Count > 0)
                {
                    builder.Append("/TestCaseFilter:\"");
                    var any = false;
                    foreach (var (assemblyInfo, testMethods) in workItemInfo.Filters)
                    {
                        foreach (var testMethod in testMethods)
                        {
                            if (any)
                            {
                                builder.Append('|');
                            }
                            builder.Append($"FullyQualifiedName={testMethod.FullyQualifiedName}");
                        }
                    }

                    builder.AppendLine();
                }

                return builder.ToString();
            }
        }
    }

}
