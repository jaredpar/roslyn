// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.Benchmarks;

_ = BenchmarkRunner.Run<CommandLineParsing>();

