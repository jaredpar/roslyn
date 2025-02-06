// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
#if NET
using System.Runtime.Loader;
using System.Threading;

#endif
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Composition;

namespace Microsoft.CodeAnalysis.ExternalAccess.Razor;

[Export(typeof(IAnalyzerResolverProvider)), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class RazorAnalyzerResolverProvider() : IAnalyzerResolverProvider
{
    private RazorAnalyzerAssemblyResolver? _resolver;

    public IAnalyzerPathResolver? GetPathResolver(AnalyzerResolverOptions options)
        => GetOrCreateResolver(options);

    private RazorAnalyzerAssemblyResolver GetOrCreateResolver(AnalyzerResolverOptions options)
    {
        if (_resolver is null)
        {
            _resolver = new RazorAnalyzerAssemblyResolver(options.RazorGeneratorFilePath);
        }
        return _resolver;
    }

#if NET

    public IAnalyzerAssemblyResolver? GetAnalyzerResolver(AnalyzerResolverOptions options)
        => GetOrCreateResolver(options);

#endif
}

internal sealed partial class RazorAnalyzerAssemblyResolver : IAnalyzerPathResolver
{
    internal static readonly string RazorSourceGeneratorSdkDirectoryFragment = CreateDirectoryPathFragment("Sdks", "Microsoft.NET.Sdk.Razor", "source-generators");

    /// <summary>
    /// When non-null all razor source generators should be unified to this directory.
    /// </summary>
    internal string? RazorGeneratorUnifyDirectory { get; }

    internal RazorAnalyzerAssemblyResolver(string? razorGeneratorDir)
    {
        // Accept either the path to the razor generator dll or the containing directory
        if (razorGeneratorDir is not null && razorGeneratorDir.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            razorGeneratorDir = Path.GetDirectoryName(razorGeneratorDir);
        }

        RazorGeneratorUnifyDirectory = razorGeneratorDir;
    }

    public bool IsAnalyzerPathHandled(string analyzerPath)
    {
        if (RazorGeneratorUnifyDirectory is null)
        {
            return false;
        }

        return
            Path.GetDirectoryName(analyzerPath) is { } p &&
            p.EndsWith(RazorSourceGeneratorSdkDirectoryFragment, StringComparison.OrdinalIgnoreCase);
    }

    private string GetUnifiedPath(string analyzerPath)
    {
        Debug.Assert(RazorGeneratorUnifyDirectory is not null);
        var fileName = Path.GetFileName(analyzerPath);
        return Path.Combine(RazorGeneratorUnifyDirectory, fileName);
    }

    public string GetRealAnalyzerPath(string analyzerPath)
        => GetUnifiedPath(analyzerPath);

    public string? GetRealSatellitePath(string analyzerPath, CultureInfo cultureInfo)
        => AnalyzerAssemblyLoader.GetSatelliteAssemblyPath(GetUnifiedPath(analyzerPath), cultureInfo);

    private static string CreateDirectoryPathFragment(params string[] paths) => Path.Combine([" ", .. paths, " "]).Trim();
}

#if NET

internal sealed partial class RazorAnalyzerAssemblyResolver : IAnalyzerAssemblyResolver
{
    internal const string RazorCompilerAssemblyName = "Microsoft.CodeAnalysis.Razor.Compiler";
    internal const string RazorUtilsAssemblyName = "Microsoft.AspNetCore.Razor.Utilities.Shared";
    internal const string ObjectPoolAssemblyName = "Microsoft.Extensions.ObjectPool";

    internal static readonly ImmutableArray<string> RazorAssemblyNames = [RazorCompilerAssemblyName, RazorUtilsAssemblyName, ObjectPoolAssemblyName];
    internal static readonly ImmutableArray<string> RazorAssemblyFileNames = RazorAssemblyNames.SelectAsArray(name => $"{name}.dll");

    private static AssemblyLoadContext? s_generatorLoadContext;

    /// <summary>
    /// This is the entry point of the razor code base to get unified assembly resolution for razor assemblies.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="rootDirectory"></param>
    /// <returns></returns>
    public static Assembly? ResolveRazorAssembly(AssemblyName assemblyName, string rootDirectory)
    {
        if (assemblyName.Name is not (RazorCompilerAssemblyName or RazorUtilsAssemblyName or ObjectPoolAssemblyName))
        {
            return null;
        }

        var context = GetOrInitializeContext(rootDirectory);
        return context.LoadFromAssemblyName(assemblyName);
    }

    /// <summary>
    /// Get the load context for the razor generator assemblies. This is a "first one wins" situation in terms
    /// of which directory the generator is loaded from.
    /// </summary>
    private static AssemblyLoadContext GetOrInitializeContext(string generatorDirectory)
    {
        if (s_generatorLoadContext is not null)
        {
            return s_generatorLoadContext;
        }

        var generator = new AssemblyLoadContext("Razor Generator Context", isCollectible: true);
        foreach (var RazorAssemblyFileName in RazorAssemblyFileNames)
        {
            var assemblyPath = Path.Combine(generatorDirectory, RazorAssemblyFileName);
            _ = generator.LoadFromAssemblyPath(assemblyPath);
        }

        if (Interlocked.CompareExchange(ref s_generatorLoadContext, generator, null) is not null)
        {
            generator.Unload();
        }

        return s_generatorLoadContext;
    }

    public Assembly? Resolve(AnalyzerAssemblyLoader loader, AssemblyName assemblyName, AssemblyLoadContext directoryContext, string directory) =>
        ResolveRazorAssembly(assemblyName, RazorGeneratorUnifyDirectory ?? directory);
}

#endif

