// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;

#if NET
using System.Runtime.Loader;
#endif

namespace Microsoft.CodeAnalysis.Host;

internal sealed class AnalyzerResolverOptions
{
    public string? RazorGeneratorFilePath { get; init; }
}

internal interface IAnalyzerResolverOptionsProvider : IWorkspaceService
{
    Task<AnalyzerResolverOptions> GetAnalyzerResolverOptionsAsync();
}

internal interface IAnalyzerAssemblyLoaderProviderFactory : IWorkspaceService
{
    IAnalyzerAssemblyLoaderProvider GetOrCreate(AnalyzerResolverOptions options);
}

internal interface IAnalyzerAssemblyLoaderProvider
{
    IAnalyzerAssemblyLoaderInternal CreateSharedShadowCopyLoader();

#if NET
    /// <summary>
    /// Creates a fresh shadow copying loader that will load all <see cref="AnalyzerReference"/>s and <see
    /// cref="ISourceGenerator"/>s in a fresh <see cref="AssemblyLoadContext"/>.
    /// </summary>
    IAnalyzerAssemblyLoaderInternal CreateNewShadowCopyLoader();
#endif
}

internal interface IAnalyzerResolverProvider
{
    IAnalyzerPathResolver? GetPathResolver(AnalyzerResolverOptions options);

#if NET
    IAnalyzerAssemblyResolver? GetAnalyzerResolver(AnalyzerResolverOptions options);
#endif
}

/// <summary>
/// Abstract implementation of an analyzer assembly loader that can be used by VS/VSCode to provide a <see
/// cref="IAnalyzerAssemblyLoader"/> with an appropriate path.
/// </summary>
internal abstract class AbstractAnalyzerAssemblyLoaderProvider : IAnalyzerAssemblyLoaderProvider
{
    private readonly Lazy<IAnalyzerAssemblyLoaderInternal> _shadowCopyLoader;
    private readonly ImmutableArray<IAnalyzerPathResolver> _pathResolvers;
#if NET
    private readonly ImmutableArray<IAnalyzerAssemblyResolver> _assemblyResolvers;
#endif

    public IAnalyzerAssemblyLoaderInternal SharedShadowCopyLoader => _shadowCopyLoader.Value;

    protected AbstractAnalyzerAssemblyLoaderProvider(
        AnalyzerResolverOptions options,
        IEnumerable<IAnalyzerResolverProvider> providers)
    {
        var pathResolvers = new List<IAnalyzerPathResolver>();
        foreach (var provider in providers)
        {
            if (provider.GetPathResolver(options) is { } p)
            {
                pathResolvers.Add(p);
            }
        }

        _pathResolvers = [.. pathResolvers];

#if NET

        var assemblyResolvers = new List<IAnalyzerAssemblyResolver>();
        foreach (var provider in providers)
        {
            if (provider.GetAnalyzerResolver(options) is { } a)
            {
                assemblyResolvers.Add(a);
            }
        }

        _assemblyResolvers = [.. assemblyResolvers];

#endif

        _shadowCopyLoader = new(CreateNewShadowCopyLoader);
    }

    public IAnalyzerAssemblyLoaderInternal CreateNewShadowCopyLoader()
    {
        return WrapLoader(Create());

        IAnalyzerAssemblyLoaderInternal Create()
        {
            var shadowPath = Path.Combine(Path.GetTempPath(), nameof(Roslyn), "AnalyzerAssemblyLoader");
#if NET
            return AnalyzerAssemblyLoader.CreateNonLockingLoader(shadowPath, _pathResolvers, _assemblyResolvers);
#else
            return AnalyzerAssemblyLoader.CreateNonLockingLoader(shadowPath, _pathResolvers);
#endif
        }
    }

    protected abstract IAnalyzerAssemblyLoaderInternal WrapLoader(IAnalyzerAssemblyLoaderInternal loader);
}

internal sealed class DefaultAnalyzerAssemblyLoaderProvider(IEnumerable<IAnalyzerResolverProvider> providers) 
    : AbstractAnalyzerAssemblyLoaderProvider(new(), providers)
{
    protected override IAnalyzerAssemblyLoaderInternal WrapLoader(IAnalyzerAssemblyLoaderInternal loader)
        => loader;
}

[ExportWorkspaceService(typeof(IAnalyzerAssemblyLoaderProviderFactory)), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class DefaultAnalyzerAssemblyLoaderProviderFactory([ImportMany] IEnumerable<IAnalyzerResolverProvider> providers) : IAnalyzerAssemblyLoaderProviderFactory
{
    private readonly DefaultAnalyzerAssemblyLoaderProvider _loaderProvider = new(providers);

    public Task<IAnalyzerAssemblyLoaderProvider> GetOrcrea()
        => Task.FromResult<IAnalyzerAssemblyLoaderProvider>(_loaderProvider);
}
