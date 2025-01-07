using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Tools;

public sealed class TestAssemblyInfo(string assemblyFilePath)
{
    public string AssemblyFilePath { get; } = assemblyFilePath;
    public string TargetFramework { get; } = AssemblyTestUtil.GetAssemblyTargetFramework(assemblyFilePath);
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

public static class AssemblyTestUtil
{
    /// <summary>
    /// Regex patterns for test assemblies that are part of the core compiler solution.
    /// </summary>
    private static readonly ImmutableArray<Regex> s_compilerTestAssemblies =
    [
        new (@"^Microsoft\.CodeAnalysis\.UnitTests$"),
        new (@"^Microsoft\.CodeAnalysis\.CompilerServer\.UnitTests$"),
        new (@"^Microsoft\.CodeAnalysis\.(CSharp|VisualBasic)\.(Syntax|Symbol|Semantic|Emit|IOperation|CommandLine)\d*\.UnitTests$"),
        new (@"^Roslyn\.Compilers\.VisualBasic\.IOperation\.UnitTests$"),
    ];

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

    public static IEnumerable<TestAssemblyInfo> GetTestAssemblies(string artifactsDir, string configuration, string filter)
    {

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
