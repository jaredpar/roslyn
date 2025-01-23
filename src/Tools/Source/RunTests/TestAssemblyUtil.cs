// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace RunTests;

internal enum TestAssemblyGroup
{
    Unit,
    Compiler,
    Core,
    Desktop,
    Integration,
}

internal enum TestAssemblyArch
{
    All,
    x86,
    x64
}

internal enum TestEnvironment
{
    Default,
    IOperation,
    UsedAssemblies
}

internal sealed class TestAssemblyUtil
{
    private static ImmutableArray<Regex> CompilerTestAssemblyRegexes { get; } =
    [
        new Regex(@"^Microsoft\.CodeAnalysis\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CompilerServer\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.Syntax\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.Symbol\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.Semantic\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.Emit\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.Emit2\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.Emit3\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.IOperation\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.CSharp\.CommandLine\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.VisualBasic\.Syntax\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.VisualBasic\.Symbol\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.VisualBasic\.Semantic\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.VisualBasic\.Emit\.UnitTests$"),
        new Regex(@"^Roslyn\.Compilers\.VisualBasic\.IOperation\.UnitTests$"),
        new Regex(@"^Microsoft\.CodeAnalysis\.VisualBasic\.CommandLine\.UnitTests$"),
    ];

    internal static bool IsMatch(string assemblyName, string tfmName, TestAssemblyGroup group) => group switch
    {
        TestAssemblyGroup.Unit => assemblyName.EndsWith(".UnitTests", StringComparison.OrdinalIgnoreCase),
        TestAssemblyGroup.Compiler => CompilerTestAssemblyRegexes.Any(r => r.IsMatch(assemblyName)),
        TestAssemblyGroup.Core => tfmName != "net472",
        TestAssemblyGroup.Desktop => tfmName == "net472",
        TestAssemblyGroup.Integration => assemblyName.EndsWith(".IntegrationTests", StringComparison.OrdinalIgnoreCase),
        _ => throw new InvalidOperationException($"Unexpected {nameof(TestAssemblyGroup)} value: {group}"),
    };

    internal static bool IsMatch(string assemblyName, TestAssemblyArch arch) => arch switch
    {
        TestAssemblyArch.All => true,
        TestAssemblyArch.x86 => assemblyName.Contains("x86"),
        TestAssemblyArch.x64 => !assemblyName.Contains(".InteractiveHost", StringComparison.OrdinalIgnoreCase),
        _ => throw new InvalidOperationException($"Unexpected {nameof(TestAssemblyArch)} value: {arch}"),
    };

    internal static bool IsMatch(string assemblyName, string tfmName, TestAssemblyGroup group, TestAssemblyArch arch) =>
        IsMatch(assemblyName, tfmName, group) && IsMatch(assemblyName, arch);

    internal static IEnumerable<string> GetTestAssemblyFilePaths(
        string artifactsDir,
        string configuration,
        TestAssemblyGroup group,
        TestAssemblyArch arch)
    {
        var list = new List<string>();
        var binDirectory = Path.Combine(artifactsDir, "bin");
        foreach (var project in Directory.EnumerateDirectories(binDirectory, "*", SearchOption.TopDirectoryOnly))
        {
            var assemblyName = Path.GetFileName(project);
            var assemblyFileName = $"{assemblyName}.dll";
            var configDirectory = Path.Combine(project, configuration);
            if (!Directory.Exists(configDirectory))
            {
                continue;
            }

            foreach (var tfmDir in Directory.EnumerateDirectories(configDirectory))
            {
                var tfmName = Path.GetFileName(tfmDir)!;
                var assemblyFilePath = Path.Combine(tfmDir, assemblyFileName);
                if (!File.Exists(assemblyFilePath))
                {
                    continue;
                }

                if (IsMatch(assemblyName, tfmName, group, arch))
                {
                    list.Add(assemblyFilePath);
                }
            }
        }

        return list;
    }

    internal static (TestAssemblyGroup Group, TestAssemblyArch Arch, TestEnvironment Environment) ParseTestConfig(string? testConfig)
    {
        var group = TestAssemblyGroup.Unit;
        var arch = TestAssemblyArch.All;
        var environment = TestEnvironment.Default;

        if (string.IsNullOrEmpty(testConfig))
        {
            return (group, arch, environment);
        }

        Span<Range> ranges = stackalloc Range[3];
        var count = testConfig.AsSpan().Split(ranges, '+', StringSplitOptions.RemoveEmptyEntries);
        if (count == 0)
        {
            throw new ArgumentException($"Invalid test configuration: {testConfig}", nameof(testConfig));
        }

        var groupSpan = testConfig[ranges[0]];
        group = groupSpan == "_" ? TestAssemblyGroup.Unit : Enum.Parse<TestAssemblyGroup>(groupSpan, ignoreCase: true);

        if (count > 1)
        {
            var archSpan = testConfig[ranges[1]];
            arch = archSpan == "_" ? TestAssemblyArch.All : Enum.Parse<TestAssemblyArch>(archSpan, ignoreCase: true);
        }

        if (count > 2)
        {
            var configSpan = testConfig[ranges[2]];
            environment = configSpan == "_" ? TestEnvironment.Default : Enum.Parse<TestEnvironment>(configSpan, ignoreCase: true);
        }

        return (group, arch, environment);
    }

    internal static string AsPlatformString(TestAssemblyArch arch) => arch switch
    {
        TestAssemblyArch.x86 => "x86",
        _ => "x64"
    };

    internal static string? AsEnvironmentVariableName(TestEnvironment environment) => environment switch
    {
        TestEnvironment.IOperation => "ROSLYN_TEST_IOPERATION",
        TestEnvironment.UsedAssemblies => "ROSLYN_TEST_USEDASSEMBLIES",
        _ => null,
    };
}
