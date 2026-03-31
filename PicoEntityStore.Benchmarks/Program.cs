using BenchmarkDotNet.Running;
using PicoEntityStore.Benchmarks;

var summary = BenchmarkRunner.Run<StoreBenchmarks>();
