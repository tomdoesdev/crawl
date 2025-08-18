using BenchmarkDotNet.Running;

namespace Crawl.Benchmark;

class Program
{
    static void Main(string[] args)
    {
        // Run specific benchmark to test
        BenchmarkRunner.Run<SparseSetBenchmarks>();
        
        // Run all benchmarks
        // BenchmarkRunner.Run(typeof(Program).Assembly);
        
        // Or run simple benchmark:
        // BenchmarkRunner.Run<SimpleSparseSetBenchmark>();
    }
}