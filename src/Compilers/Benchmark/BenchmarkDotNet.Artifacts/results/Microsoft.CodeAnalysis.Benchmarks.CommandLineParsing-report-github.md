```

BenchmarkDotNet v0.13.8, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2)
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK 8.0.100-preview.7.23376.3
  [Host]     : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

Error=4.71 μs  StdDev=4.40 μs  

```
| Method | Mean     | Allocated |
|------- |---------:|----------:|
| Parse  | 389.8 μs | 168.72 KB |
