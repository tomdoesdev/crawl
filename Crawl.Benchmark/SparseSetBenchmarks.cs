using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Crawl.DataStructures;
using Crawl.ECS;
using Crawl.ECS.Components;
using Crawl.ECS.Entities;

namespace Crawl.Benchmark;

// Test components for benchmarking
public readonly struct PositionComponent(float x, float y) : IComponent
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public ComponentType ComponentType => ComponentType.Position;
}

public readonly struct VelocityComponent(float dx, float dy) : IComponent
{
    public float DX { get; } = dx;
    public float DY { get; } = dy;
    public ComponentType ComponentType => ComponentType.Velocity;
}

public readonly struct HealthComponent(int current, int max) : IComponent
{
    public int Current { get; } = current;
    public int Max { get; } = max;
    public ComponentType ComponentType => ComponentType.Health;
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[RankColumn]
public class SparseSetBenchmarks
{
    private SparseSet<PositionComponent> _sparseSet = null!;
    private SparseSet<PositionComponent> _populatedSparseSet = null!;
    private Entity[] _entities = null!;
    private PositionComponent[] _positions = null!;
    private uint[] _randomEntityIds = null!;

    [Params(1000, 10000, 100000)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Initialize empty sparse set
        _sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        
        // Create test data
        _entities = new Entity[EntityCount];
        _positions = new PositionComponent[EntityCount];
        
        var random = new Random(42); // Fixed seed for reproducibility
        
        for (int i = 0; i < EntityCount; i++)
        {
            _entities[i] = new Entity((uint)(i + 1));
            _positions[i] = new PositionComponent(
                random.NextSingle() * 1000f, 
                random.NextSingle() * 1000f
            );
        }
        
        // Create random entity IDs for lookup tests - same count as EntityCount for fair comparison
        _randomEntityIds = new uint[EntityCount];
        for (int i = 0; i < EntityCount; i++)
        {
            _randomEntityIds[i] = (uint)(random.Next(EntityCount) + 1);
        }
        
        // Create pre-populated sparse set for read/remove tests
        _populatedSparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        for (int i = 0; i < EntityCount; i++)
        {
            _populatedSparseSet.Add(_entities[i], _positions[i]);
        }
    }

    #region Add Benchmarks

    [Benchmark]
    public void Add_Sequential()
    {
        var sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Add(_entities[i], _positions[i]);
        }
    }

    [Benchmark]
    public void Add_WithGrowth()
    {
        // Start with small capacity to force multiple growths
        var sparseSet = new SparseSet<PositionComponent>(100, 500);
        
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Add(_entities[i], _positions[i]);
        }
    }

    [Benchmark]
    public void TryAdd_Sequential()
    {
        var sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.TryAdd(_entities[i], _positions[i]);
        }
    }

    [Benchmark]
    public void TryAdd_WithDuplicates()
    {
        var sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        
        // Add each entity twice (second add should fail)
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.TryAdd(_entities[i], _positions[i]);
            sparseSet.TryAdd(_entities[i], _positions[i]); // Should return false
        }
    }

    #endregion

    #region Get Benchmarks

    [Benchmark]
    public void Get_Sequential()
    {
        var sum = 0f;
        
        for (int i = 0; i < EntityCount; i++)
        {
            var pos = _populatedSparseSet.Get(_entities[i]);
            sum += pos.X + pos.Y;
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void Get_Random()
    {
        var sum = 0f;
        
        for (int i = 0; i < EntityCount; i++)
        {
            var entity = new Entity(_randomEntityIds[i]);
            var pos = _populatedSparseSet.Get(entity);
            sum += pos.X + pos.Y;
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void TryGet_Sequential()
    {
        var sum = 0f;
        
        for (int i = 0; i < EntityCount; i++)
        {
            if (_populatedSparseSet.TryGet(_entities[i], out var pos))
            {
                sum += pos.X + pos.Y;
            }
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void TryGet_Random()
    {
        var sum = 0f;
        
        for (int i = 0; i < EntityCount; i++)
        {
            var entity = new Entity(_randomEntityIds[i]);
            if (_populatedSparseSet.TryGet(entity, out var pos))
            {
                sum += pos.X + pos.Y;
            }
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }

    #endregion

    #region Has Benchmarks

    [Benchmark]
    public void Has_Sequential()
    {
        var count = 0;
        
        for (int i = 0; i < EntityCount; i++)
        {
            if (_populatedSparseSet.Has(_entities[i]))
            {
                count++;
            }
        }
        
        // Prevent optimization
        if (count != EntityCount) throw new InvalidOperationException();
    }

    [Benchmark]
    public void Has_Random()
    {
        var count = 0;
        
        for (int i = 0; i < EntityCount; i++)
        {
            var entity = new Entity(_randomEntityIds[i]);
            if (_populatedSparseSet.Has(entity))
            {
                count++;
            }
        }
        
        // Prevent optimization
        if (count < 0) throw new InvalidOperationException();
    }

    #endregion

    #region Remove Benchmarks

    [Benchmark]
    public void Remove_Sequential()
    {
        var sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        
        // First populate it
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Add(_entities[i], _positions[i]);
        }
        
        // Then remove all entities
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Remove(_entities[i]);
        }
    }

    [Benchmark]
    public void Remove_EveryOther()
    {
        var sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        
        // First populate it
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Add(_entities[i], _positions[i]);
        }
        
        // Remove every other entity (tests swap-with-last behavior)
        for (int i = 0; i < EntityCount; i += 2)
        {
            sparseSet.Remove(_entities[i]);
        }
    }

    [Benchmark]
    public void Remove_Random()
    {
        var sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        var random = new Random(42);
        
        // First populate it
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Add(_entities[i], _positions[i]);
        }
        
        // Remove entities in random order
        var entitiesToRemove = new Entity[EntityCount / 2];
        var usedIndices = new HashSet<int>();
        
        for (int i = 0; i < EntityCount / 2; i++)
        {
            int index;
            do
            {
                index = random.Next(EntityCount);
            } while (usedIndices.Contains(index));
            
            usedIndices.Add(index);
            entitiesToRemove[i] = _entities[index];
        }
        
        foreach (var entity in entitiesToRemove)
        {
            sparseSet.Remove(entity);
        }
    }

    [Benchmark]
    public void Remove_NonExistent()
    {
        // Test removal of entities that don't exist - scale with EntityCount but keep reasonable
        var testCount = Math.Min(1000, EntityCount / 10); // At most 1000, but scale down for smaller sets
        for (int i = 0; i < testCount; i++)
        {
            var nonExistentEntity = new Entity((uint)(EntityCount + i + 1));
            _populatedSparseSet.Remove(nonExistentEntity);
        }
    }

    #endregion

    #region Iteration Benchmarks

    [Benchmark]
    public void GetAll_Iterate()
    {
        var sum = 0f;
        var components = _populatedSparseSet.GetAll();
        
        for (int i = 0; i < components.Length; i++)
        {
            sum += components[i].X + components[i].Y;
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void GetAll_IterateWithSpan()
    {
        var sum = 0f;
        var components = _populatedSparseSet.GetAll();
        
        foreach (var component in components)
        {
            sum += component.X + component.Y;
        }
        
        // Prevent optimization
        if (sum < 0) throw new InvalidOperationException();
    }

    #endregion

    #region Mixed Operation Benchmarks

    [Benchmark]
    public void MixedOperations_AddRemoveGet()
    {
        var sparseSet = new SparseSet<PositionComponent>(1000, 500);
        var random = new Random(42);
        var sum = 0f;
        
        for (int i = 0; i < 10000; i++)
        {
            var operation = random.Next(3);
            var entityId = (uint)(random.Next(EntityCount) + 1);
            var entity = new Entity(entityId);
            
            switch (operation)
            {
                case 0: // Add
                    sparseSet.TryAdd(entity, new PositionComponent(i, i));
                    break;
                case 1: // Remove
                    sparseSet.Remove(entity);
                    break;
                case 2: // Get
                    if (sparseSet.TryGet(entity, out var pos))
                    {
                        sum += pos.X;
                    }
                    break;
            }
        }
        
        // Prevent optimization
        if (sum < -1000000) throw new InvalidOperationException();
    }

    [Benchmark]
    public void Churn_AddRemoveCycle()
    {
        var sparseSet = new SparseSet<PositionComponent>(1000, 500);
        
        // Simulate high churn - constantly adding and removing entities
        for (int cycle = 0; cycle < 100; cycle++)
        {
            // Add a bunch of entities
            for (int i = 0; i < 1000; i++)
            {
                var entity = new Entity((uint)(cycle * 1000 + i + 1));
                sparseSet.TryAdd(entity, new PositionComponent(i, cycle));
            }
            
            // Remove half of them
            for (int i = 0; i < 500; i++)
            {
                var entity = new Entity((uint)(cycle * 1000 + i + 1));
                sparseSet.Remove(entity);
            }
        }
    }

    #endregion

    #region Clear Benchmark

    [Benchmark]
    public void Clear_FullSet()
    {
        var sparseSet = new SparseSet<PositionComponent>(EntityCount / 2, 1000);
        
        // Populate it
        for (int i = 0; i < EntityCount; i++)
        {
            sparseSet.Add(_entities[i], _positions[i]);
        }
        
        // Clear it
        sparseSet.Clear();
    }

    #endregion
}

// Comparison benchmark against other data structures
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[RankColumn]
public class SparseSetComparisonBenchmarks
{
    private SparseSet<PositionComponent> _sparseSet = null!;
    private Dictionary<uint, PositionComponent> _dictionary = null!;
    private List<(Entity entity, PositionComponent component)> _list = null!;
    private Entity[] _entities = null!;
    private PositionComponent[] _positions = null!;

    [Params(1000, 10000)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _entities = new Entity[EntityCount];
        _positions = new PositionComponent[EntityCount];
        
        for (int i = 0; i < EntityCount; i++)
        {
            _entities[i] = new Entity((uint)(i + 1));
            _positions[i] = new PositionComponent(i, i * 2);
        }
        
        // Initialize SparseSet
        _sparseSet = new SparseSet<PositionComponent>(EntityCount, 1000);
        for (int i = 0; i < EntityCount; i++)
        {
            _sparseSet.Add(_entities[i], _positions[i]);
        }
        
        // Initialize Dictionary
        _dictionary = new Dictionary<uint, PositionComponent>(EntityCount);
        for (int i = 0; i < EntityCount; i++)
        {
            _dictionary.Add(_entities[i].Id, _positions[i]);
        }
        
        // Initialize List
        _list = new List<(Entity, PositionComponent)>(EntityCount);
        for (int i = 0; i < EntityCount; i++)
        {
            _list.Add((_entities[i], _positions[i]));
        }
    }

    [Benchmark(Baseline = true)]
    public void SparseSet_RandomAccess()
    {
        var sum = 0f;
        var random = new Random(42);
        
        for (int i = 0; i < 1000; i++)
        {
            var entity = _entities[random.Next(EntityCount)];
            var pos = _sparseSet.Get(entity);
            sum += pos.X;
        }
        
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void Dictionary_RandomAccess()
    {
        var sum = 0f;
        var random = new Random(42);
        
        for (int i = 0; i < 1000; i++)
        {
            var entityId = _entities[random.Next(EntityCount)].Id;
            var pos = _dictionary[entityId];
            sum += pos.X;
        }
        
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void List_RandomAccess()
    {
        var sum = 0f;
        var random = new Random(42);
        
        for (int i = 0; i < 1000; i++)
        {
            var targetEntity = _entities[random.Next(EntityCount)];
            PositionComponent pos = default;
            
            for (int j = 0; j < _list.Count; j++)
            {
                if (_list[j].entity.Id == targetEntity.Id)
                {
                    pos = _list[j].component;
                    break;
                }
            }
            
            sum += pos.X;
        }
        
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void SparseSet_Iteration()
    {
        var sum = 0f;
        var components = _sparseSet.GetAll();
        
        for (int i = 0; i < components.Length; i++)
        {
            sum += components[i].X;
        }
        
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void Dictionary_Iteration()
    {
        var sum = 0f;
        
        foreach (var kvp in _dictionary)
        {
            sum += kvp.Value.X;
        }
        
        if (sum < 0) throw new InvalidOperationException();
    }

    [Benchmark]
    public void List_Iteration()
    {
        var sum = 0f;
        
        for (int i = 0; i < _list.Count; i++)
        {
            sum += _list[i].component.X;
        }
        
        if (sum < 0) throw new InvalidOperationException();
    }
}