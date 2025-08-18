using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Crawl.DataStructures;
using Crawl.ECS;
using Crawl.ECS.Components;
using Crawl.ECS.Entities;

namespace Crawl.Benchmark;

// Simple test component
public readonly struct BenchmarkComponent(int value) : IComponent
{
    public int Value { get; } = value;
    public ComponentType ComponentType => ComponentType.Position;
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class SimpleSparseSetBenchmark
{
    private SparseSet<BenchmarkComponent> _sparseSet = null!;
    private Entity[] _entities = null!;
    private BenchmarkComponent[] _components = null!;

    [Params(1000)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _sparseSet = new SparseSet<BenchmarkComponent>(EntityCount, 1000);
        _entities = new Entity[EntityCount];
        _components = new BenchmarkComponent[EntityCount];
        
        for (int i = 0; i < EntityCount; i++)
        {
            _entities[i] = new Entity((uint)(i + 1));
            _components[i] = new BenchmarkComponent(i);
        }
        
        // Pre-populate for read tests
        for (int i = 0; i < EntityCount; i++)
        {
            _sparseSet.Add(_entities[i], _components[i]);
        }
    }

    [Benchmark]
    public void Add_Test()
    {
        var sparseSet = new SparseSet<BenchmarkComponent>(EntityCount, 1000);
        
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Add(_entities[i], _components[i]);
        }
    }

    [Benchmark]
    public void Get_Test()
    {
        var sum = 0;
        
        for (int i = 0; i < EntityCount; i++)
        {
            var comp = _sparseSet.Get(_entities[i]);
            sum += comp.Value;
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void Has_Test()
    {
        var count = 0;
        
        for (int i = 0; i < EntityCount; i++)
        {
            if (_sparseSet.Has(_entities[i]))
            {
                count++;
            }
        }
        
        // Prevent optimization
        if (count != EntityCount) throw new InvalidOperationException();
    }

    [Benchmark]
    public void Iterate_Test()
    {
        var sum = 0;
        var components = _sparseSet.GetAll();
        
        for (int i = 0; i < components.Length; i++)
        {
            sum += components[i].Value;
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }
}