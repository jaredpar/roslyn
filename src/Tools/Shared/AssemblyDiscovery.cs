// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RunTests
{
    internal static class AssemblyDiscovery
    {
        internal static ImmutableArray<AssemblyInfo> GetAssemblyFilePaths(
            string artifactsDirectory,
            string configuration,
            TestRuntime testRuntime,
            List<string> includeFilter,
            List<string> excludeFilter)
        {
            var list = new List<AssemblyInfo>();
            var binDirectory = Path.Combine(artifactsDirectory, "bin");
            foreach (var project in Directory.EnumerateDirectories(binDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(project);
                if (!shouldInclude(name, includeFilter) || shouldExclude(name, excludeFilter))
                {
                    Console.WriteLine($"Skipping {name} because it is not included or is excluded");
                    continue;
                }

                var fileName = $"{name}.dll";

                var configDirectory = Path.Combine(project, configuration);
                if (!Directory.Exists(configDirectory))
                {
                    Console.WriteLine($"Skipping {name} because {configuration} does not exist");
                    continue;
                }

                foreach (var targetFrameworkDirectory in Directory.EnumerateDirectories(configDirectory))
                {
                    var tfm = Path.GetFileName(targetFrameworkDirectory);
                    if (!IsMatch(testRuntime, tfm))
                    {
                        Console.WriteLine($"Skipping {name} {tfm} does not match the target framework");
                        continue;
                    }

                    var filePath = Path.Combine(targetFrameworkDirectory, fileName);
                    if (File.Exists(filePath))
                    {
                        list.Add(new AssemblyInfo(filePath));
                    }
                    else if (Directory.GetFiles(targetFrameworkDirectory, searchPattern: "*.UnitTests.dll") is { Length: > 0 } matches)
                    {
                        // If the unit test assembly name doesn't match the project folder name, but still matches our "unit test" name pattern, we want to run it.
                        // If more than one such assembly is present in a project output folder, we assume something is wrong with the build configuration.
                        // For example, one unit test project might be referencing another unit test project.
                        if (matches.Length > 1)
                        {
                            var message = $"Multiple unit test assemblies found in '{targetFrameworkDirectory}'. Please adjust the build to prevent this. Matches:{Environment.NewLine}{string.Join(Environment.NewLine, matches)}";
                            throw new Exception(message);
                        }

                        Console.WriteLine($"Found unit test assembly '{matches[0]}' in '{targetFrameworkDirectory}'");
                        list.Add(new AssemblyInfo(matches[0]));
                    }
                    else
                    {
                        Console.WriteLine($"{targetFrameworkDirectory} does not contain unit tests");
                    }
                }
            }

            if (list.Count == 0)
            {
                throw new InvalidOperationException($"Did not find any test assemblies");
            }

            list.Sort();
            return list.ToImmutableArray();

            static bool shouldInclude(string name, List<string> includeFilter)
            {
                foreach (var pattern in includeFilter)
                {
                    if (Regex.IsMatch(name, pattern.Trim('\'', '"')))
                    {
                        return true;
                    }
                }

                return false;
            }

            static bool shouldExclude(string name, List<string> excludeFilter)
            {
                foreach (var pattern in excludeFilter)
                {
                    if (Regex.IsMatch(name, pattern.Trim('\'', '"')))
                    {
                        return true;
                    }
                }

                return false;
            }

            static bool IsMatch(TestRuntime testRuntime, string dirName) =>
                testRuntime switch
                {
                    TestRuntime.Both => IsCompatibleWithCurrentPlatform(dirName),
                    TestRuntime.Core => Regex.IsMatch(dirName, @"^net\d+\.") && IsCompatibleWithCurrentPlatform(dirName),
                    TestRuntime.Framework => dirName is "net472",
                    _ => throw new InvalidOperationException($"Unexpected {nameof(TestRuntime)} value: {testRuntime}"),
                };

            static bool IsCompatibleWithCurrentPlatform(string tfmDirName)
            {
                if (tfmDirName.EndsWith("-windows", StringComparison.Ordinal))
                {
                    return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                }

                if (tfmDirName.EndsWith("-macos", StringComparison.Ordinal))
                {
                    return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
                }

                return true;
            }
        }
    }
}
