using BenchmarkDotNet.Running;
using PicoECS.Benchmarks;

var summary = BenchmarkRunner.Run<StoreBenchmarks>();
