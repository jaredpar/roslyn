# MefCrashRepro

Minimal repro for a native crash (SIGSEGV) that occurs during MEF composition
in Roslyn's Linux CI builds. The crash hits ~10% of `roslyn-CI` builds and
only affects Linux legs.

## Background

Over a one-week sample (late May / early June 2026) we observed **14 native
crashes across 13 of 131 builds** in the `roslyn-CI` pipeline. Every crash
occurred during test host startup in a Linux Debug job. Key observations:

- All 14 crashing tests live under the
  `Microsoft.CodeAnalysis.LanguageServer.UnitTests` namespace (12 from
  `ProtocolUnitTests`, 2 from `LanguageServer.UnitTests`).
- **No test repeats** — 14 different tests crashed, ruling out a test-specific
  bug.
- No other Roslyn test assemblies crash on Linux.
- The crash rate is steady at 2-5 per day.

## Crash analysis

GC root traces from the crash dumps consistently show the crash inside
`Microsoft.VisualStudio.Composition.PartDiscovery.CreatePartsAsync`. This
method uses a TPL Dataflow pipeline internally:

```
TransformManyBlock<Assembly, Type>   ← extracts types from assemblies
    ↓
TransformBlock<Type, Object>         ← reflects on each type for MEF attributes
    ↓
ActionBlock<Object>                  ← collects DiscoveredParts
```

The pipeline runs with `MaxDegreeOfParallelism` equal to `ProcessorCount`,
meaning multiple threads concurrently reflect on types from the target
assemblies.

### Why only LSP tests?

The LSP Protocol assembly is unique in Roslyn's codebase:

- **1,351 types** including **70+ closed generic instantiations** of
  `SumType<T1,T2>`, `SumType<T1,T2,T3>`, and `SumType<T1,T2,T3,T4>`.
- Each `SumType<>` variant implements `IEquatable<>`, has implicit conversion
  operators, and multiple constructors — all of which MEF must reflect over.

The working theory is that **concurrent reflection on these complex generic
types triggers a race condition in the .NET runtime's type metadata system on
Linux**. The race is *within* a single `CreatePartsAsync` call (Roslyn tests
do not run in parallel). The large number of generic instantiations in the
Protocol assembly makes this the only assembly that reliably hits the window.

## How to run

```bash
# Default: 500 iterations, ProcessorCount concurrent workers
dotnet run --project src/Tools/MefCrashRepro/MefCrashRepro.csproj -c Debug

# Custom: 1000 iterations, 16 workers
dotnet run --project src/Tools/MefCrashRepro/MefCrashRepro.csproj -c Debug -- 1000 16
```

The program loads the same five assemblies used by the real LSP test
composition (~12,600 types total), then hammers `PartDiscovery.CreatePartsAsync`
from multiple concurrent workers. A native crash (SIGSEGV / process abort)
indicates a successful repro; if it runs to completion the bug was not
triggered.

**Note:** The crash has only been observed on Linux CI agents. It may require
specific conditions (VM memory pressure, kernel version, CPU model) to
reproduce.

## VS-MEF version

`Microsoft.VisualStudio.Composition` 18.3.36 (from `eng/Packages.props`).
