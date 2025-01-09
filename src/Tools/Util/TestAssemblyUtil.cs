// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Tools;

public sealed class TestAssemblyInfo(string assemblyFilePath)
{
    public string AssemblyFilePath { get; } = assemblyFilePath;
    public string TargetFramework { get; } = TestAssemblyUtil.GetAssemblyTargetFramework(assemblyFilePath);
    public string AssemblyFileName { get; } = Path.GetFileName(assemblyFilePath);
    public override string ToString() => $"{AssemblyFileName} ({TargetFramework})";
}

public enum TestAssemblySet
{
    Compiler,
    UnitTests,
    IntegrationTests
}

public enum TestTargetFramework
{
    All,
    Framework,
    Core
}

public static partial class TestAssemblyUtil
{
    /// <summary>
    /// Regex patterns for test assemblies that are part of the core compiler solution.
    /// </summary>
    private static ImmutableArray<Regex> CompilerTestAssembliesRegex { get; } =
    [
        new (@"^Microsoft\.CodeAnalysis\.UnitTests$"),
        new (@"^Microsoft\.CodeAnalysis\.CompilerServer\.UnitTests$"),
        new (@"^Microsoft\.CodeAnalysis\.(CSharp|VisualBasic)\.(Syntax|Symbol|Semantic|Emit|IOperation|CommandLine)\d*\.UnitTests$"),
        new (@"^Roslyn\.Compilers\.VisualBasic\.IOperation\.UnitTests$"),
    ];

    [GeneratedRegex(@"^net\d+\.\d+$")]
    private static partial Regex CoreTargetFrameworkRegex { get; }

    public static bool IsMatch(TestAssemblyInfo testAssemblyInfo, TestAssemblySet testAssemblySet) => testAssemblySet switch
    {
        TestAssemblySet.Compiler => CompilerTestAssembliesRegex.Any(r => r.IsMatch(testAssemblyInfo.AssemblyFileName)),
        TestAssemblySet.UnitTests => testAssemblyInfo.AssemblyFileName.EndsWith(".UnitTests.dll", StringComparison.Ordinal),
        TestAssemblySet.IntegrationTests => testAssemblyInfo.AssemblyFileName.EndsWith(".IntegrationTests.dll", StringComparison.Ordinal),
        _ => throw new ArgumentException($"Unknown test assembly set: {testAssemblySet}", nameof(testAssemblySet))
    };

    public static bool IsMatch(TestAssemblyInfo testAssemblyInfo, TestTargetFramework testTargetFramework) => testTargetFramework switch
    {
        TestTargetFramework.All => true,
        TestTargetFramework.Framework => testAssemblyInfo.TargetFramework == "net472",
        TestTargetFramework.Core => CoreTargetFrameworkRegex.IsMatch(testAssemblyInfo.TargetFramework),
        _ => throw new ArgumentException($"Unknown test target framework: {testTargetFramework}", nameof(testTargetFramework))
    };

    public static (TestAssemblySet TestAssemblySet, TestTargetFramework TestTargetFramework) ParseFilter(ReadOnlySpan<char> filter)
    {
        var index = filter.IndexOf('+');
        TestTargetFramework testTargetFramework;
        if (index < 0)
        {
            testTargetFramework = TestTargetFramework.All;
        }
        else
        {
            testTargetFramework = Enum.Parse<TestTargetFramework>(filter[(index + 1)..], ignoreCase: true);
            filter = filter[0..index];
        }

        var testAssemblySet = Enum.Parse<TestAssemblySet>(filter, ignoreCase: true);
        return (testAssemblySet, testTargetFramework);
    }

    public static IEnumerable<TestAssemblyInfo> GetTestAssemblies(
        string artifactsDir,
        string configuration,
        TestAssemblySet testAssemblySet,
        TestTargetFramework testTargetFramework) =>
        Filter(GetTestAssemblies(artifactsDir, configuration), testAssemblySet, testTargetFramework);

    public static IEnumerable<TestAssemblyInfo> Filter(IEnumerable<TestAssemblyInfo> testAssemblyInfos, TestAssemblySet testAssemblySet, TestTargetFramework testTargetFramework)
    {
        foreach (var testAssemblyInfo in testAssemblyInfos)
        {
            if (IsMatch(testAssemblyInfo, testAssemblySet) && IsMatch(testAssemblyInfo, testTargetFramework))
            {
                yield return testAssemblyInfo;
            }
        }
    }

    public static IEnumerable<TestAssemblyInfo> GetTestAssemblies(string artifactsDir, string configuration, string filter)
    {
        var (testAssemblySet, testTargetFramework) = ParseFilter(filter);
        return GetTestAssemblies(artifactsDir, configuration, testAssemblySet, testTargetFramework);
    }

    public static IEnumerable<TestAssemblyInfo> GetTestAssemblies(string artifactsDir, string configuration)
    {
        var binDir = Path.Combine(artifactsDir, "bin");
        foreach (var d in Directory.EnumerateDirectories(binDir, "*Tests"))
        {
            if (d.EndsWith(".UnitTests", StringComparison.Ordinal) || d.EndsWith(".IntegrationTests", StringComparison.Ordinal))
            {
                var assemblyName = $"{Path.GetFileName(d)}.dll";
                var configDir = Path.Combine(d, configuration);
                foreach (var f in Directory.EnumerateFiles(configDir, assemblyName, SearchOption.AllDirectories))
                {
                    yield return new TestAssemblyInfo(f);
                }
            }
        }
    }

    public static string GetAssemblyTargetFramework(string assemblyPath)
    {
        var dir = Path.GetDirectoryName(assemblyPath)!;
        var targetFramework = Path.GetFileName(dir)!;
        return targetFramework;
    }
}
