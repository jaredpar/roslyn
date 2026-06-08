using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;

// This repro mimics what the Roslyn test infrastructure does:
// 1. Creates a PartDiscovery (same config as ExportProviderCache.CreatePartDiscovery)
// 2. Calls CreatePartsAsync with the LSP Protocol assembly (which has 70+ SumType<> generic instantiations)
// 3. Repeats in a loop to try to hit the native crash seen on Linux CI
//
// The crash is a native SIGSEGV inside PartDiscovery.CreatePartsAsync on Linux.
// The theory is that TPL Dataflow's internal parallelism in PartDiscovery causes
// concurrent reflection on complex generic types (SumType<T1,T2,T3,T4>) which
// triggers a race condition in the .NET runtime's type system on Linux.

// Load the Protocol assembly — it's a project reference so it's already loadable
var lspProtocolAssembly = AppDomain.CurrentDomain.GetAssemblies()
    .FirstOrDefault(a => a.GetName().Name == "Microsoft.CodeAnalysis.LanguageServer.Protocol")
    ?? Assembly.Load("Microsoft.CodeAnalysis.LanguageServer.Protocol");

Console.WriteLine($"Target assembly: {lspProtocolAssembly.GetName().Name}");
Console.WriteLine($"Type count: {lspProtocolAssembly.GetTypes().Length}");
Console.WriteLine($"Runtime: {Environment.Version}");
Console.WriteLine($"OS: {Environment.OSVersion}");
Console.WriteLine($"Processors: {Environment.ProcessorCount}");
Console.WriteLine();

var iterations = 500;
var concurrency = Environment.ProcessorCount;

if (args.Length > 0 && int.TryParse(args[0], out var n))
    iterations = n;
if (args.Length > 1 && int.TryParse(args[1], out var c))
    concurrency = c;

Console.WriteLine($"Running {iterations} iterations with {concurrency} concurrent workers...");
Console.WriteLine();

// Also load additional assemblies that the real test composition uses
var assemblies = new List<Assembly> { lspProtocolAssembly };
var additionalNames = new[]
{
    "Microsoft.CodeAnalysis.Features",
    "Microsoft.CodeAnalysis.Workspaces",
    "Microsoft.CodeAnalysis.CSharp.Features",
    "Microsoft.CodeAnalysis.CSharp.Workspaces",
};

foreach (var name in additionalNames)
{
    try
    {
        var asm = Assembly.Load(name);
        assemblies.Add(asm);
        Console.WriteLine($"  Loaded: {asm.GetName().Name} ({asm.GetTypes().Length} types)");
    }
    catch
    {
        Console.WriteLine($"  Skipped: {name} (not available)");
    }
}

Console.WriteLine();

var completed = 0;
var crashed = 0;
var sw = Stopwatch.StartNew();

// Run concurrent MEF discovery operations to maximize thread contention
// on the runtime's type system (concurrent reflection on generic types)
var tasks = Enumerable.Range(0, concurrency).Select(worker => Task.Run(async () =>
{
    while (true)
    {
        var iteration = Interlocked.Increment(ref completed);
        if (iteration > iterations)
            break;

        try
        {
            // Create fresh PartDiscovery each time (like ExportProviderBuilder does)
            var resolver = Resolver.DefaultInstance;
            var discovery = PartDiscovery.Combine(
                new AttributedPartDiscoveryV1(resolver),
                new AttributedPartDiscovery(resolver, isNonPublicSupported: true));

            var parts = await discovery.CreatePartsAsync(assemblies);

            if (iteration % 50 == 0 || iteration == 1)
            {
                Console.WriteLine($"[{iteration}/{iterations}] Worker {worker}: {parts.Parts.Count} parts, {parts.DiscoveryErrors.Count} errors");
            }
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref crashed);
            Console.WriteLine($"[{iteration}/{iterations}] Worker {worker} EXCEPTION: {ex.GetType().Name}: {ex.Message}");
        }
    }
})).ToArray();

await Task.WhenAll(tasks);
sw.Stop();

Console.WriteLine();
Console.WriteLine($"Completed {iterations} iterations in {sw.Elapsed.TotalSeconds:F1}s ({crashed} exceptions)");
Console.WriteLine("If no native crash occurred, the repro did not trigger the bug.");
