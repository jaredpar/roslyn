// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ICSharpCode.Decompiler.Util;
using Microsoft.TeamFoundation.Core.WebApi;
using Mono.Options;

namespace HelixUtil;

internal class Options
{
    public required string ArtifactsDirectory { get; set;}
    public required string HelixQueueName { get; set; }
    public required string HelixJobName { get; set; }
    public required string Architecture { get; set; }
    public required OSPlatform Platform { get; set; }

    internal static Options? Parse(string[] args)
    {
        var architecture = "x64";
        string? helixJobName = null;
        string? helixQueueName = null;
        OSPlatform? platform = null;
        string? artifactsDirectory = null;
        var optionSet = new OptionSet()
        {
            { "artifacts=", "Path to the artifacts directory", (string s) => artifactsDirectory = s },
            { "helixQueueName=", "Name of the Helix queue to run tests on", (string s) => helixQueueName = s },
            { "helixJobName=", "Name of the Helix job", (string s) => helixJobName = s },
            { "architecture=", "Architecture to run tests on (x64, x86, arm, arm64)", (string s) => architecture = s },
            { "platform=", "Platform to run tests on", (string s) => platform = OSPlatform.Create(s) },
        };

        List<string> assemblyList;
        try
        {
            assemblyList = optionSet.Parse(args);
        }
        catch (OptionException e)
        {
            ConsoleUtil.WriteLine($"Error parsing command line arguments: {e.Message}");
            optionSet.WriteOptionDescriptions(Console.Out);
            return null;
        }

        if (string.IsNullOrEmpty(architecture))
        {
            ConsoleUtil.WriteLine($"Missing or invalid value for {nameof(architecture)}");
            return null;
        }

        if (string.IsNullOrEmpty(helixJobName))
        {
            ConsoleUtil.WriteLine($"Missing or invalid value for {nameof(helixJobName)}");
            return null;
        }

        if (string.IsNullOrEmpty(helixQueueName))
        {
            ConsoleUtil.WriteLine($"Missing or invalid value for {nameof(helixQueueName)}");
            return null;
        }

        if (string.IsNullOrEmpty(artifactsDirectory))
        {
            artifactsDirectory = TryGetArtifactsPath();
        }

        if (string.IsNullOrEmpty(artifactsDirectory) || !Directory.Exists(artifactsDirectory))
        {
            ConsoleUtil.WriteLine($"Did not find artifacts directory at {artifactsDirectory}");
            return null;

        }

        if (platform is null)
        {
            ConsoleUtil.WriteLine($"Missing or invalid value for {nameof(platform)}");
            return null;
        }

        return new Options()
        {
            ArtifactsDirectory = artifactsDirectory,
            HelixQueueName = helixQueueName,
            HelixJobName = helixJobName,
            Architecture = architecture,
            Platform = platform.Value,
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
    }
}
