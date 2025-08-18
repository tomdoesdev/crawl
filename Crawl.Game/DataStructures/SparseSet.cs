using Crawl.ECS.Components;
using Crawl.ECS.Entities;
using System.Runtime.CompilerServices;

namespace Crawl.DataStructures;

public class DuplicateComponentException : Exception
{

    public DuplicateComponentException(string message) : base(message) { }

    public DuplicateComponentException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class ComponentNotFoundException : Exception
{
    public ComponentNotFoundException() : base() { }

    public ComponentNotFoundException(string message) : base(message) { }

    public ComponentNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Cache-optimized entry combining entity and component data for better memory layout
/// </summary>
public struct SparseSetEntry<T> where T : struct
{
    public Entity Entity;
    public T Component;
    
    public SparseSetEntry(Entity entity, T component)
    {
        Entity = entity;
        Component = component;
    }
}

/// <summary>
/// A true sparse set implementation optimized for cache performance.
/// Uses a single packed array for optimal memory layout and cache locality.
/// </summary>
public class SparseSet<T> where T : struct, IComponent
{
    // Single dense array for optimal cache performance - all data packed together
    private SparseSetEntry<T>[] _dense;
    
    // Sparse array - maps entity ID directly to dense array index
    private readonly int[] _sparse;
    
    private int _count;
    private int _capacity;
    private readonly int _growCapacity;
    private readonly uint _maxEntityId;

    // Cache-friendly field layout - group frequently accessed fields together
    private readonly uint _sparseArraySize;

    /// <summary>
    /// Creates a new cache-optimized sparse set.
    /// </summary>
    /// <param name="initialCapacity">Initial capacity for dense array</param>
    /// <param name="growCapacity">Amount to grow dense array when needed</param>
    /// <param name="maxEntityId">Maximum entity ID this sparse set can handle</param>
    public SparseSet(int initialCapacity = 1000, int growCapacity = 1000, uint maxEntityId = 100000)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("initial capacity must be greater than 0");
        }

        if (growCapacity <= 0)
        {
            throw new ArgumentException("grow capacity must be greater than 0");
        }
        
        // Align initial capacity to cache line boundaries for better performance
        var alignedCapacity = AlignToPage(initialCapacity);
        
        _dense = new SparseSetEntry<T>[alignedCapacity];
        _capacity = alignedCapacity;
        _growCapacity = growCapacity;
        _maxEntityId = maxEntityId;
        _sparseArraySize = maxEntityId + 1;
        
        // Allocate sparse array with proper alignment
        _sparse = new int[_sparseArraySize];
        
        // Initialize sparse array with invalid indices (-1 means "not present")
        // Use optimized fill for better performance on large arrays
        Array.Fill(_sparse, -1);
        
        _count = 0;
    }

    /// <summary>
    /// Adds a component for the specified entity. Throws if entity already has this component.
    /// </summary>
    public void Add(Entity entity, T value)
    {
        ValidateEntityId(entity.Id);
        
        if (Has(entity))
            throw new DuplicateComponentException($"Entity {entity.Id} already has component {typeof(T).Name}");

        EnsureCapacity(_count + 1);
        
        // Single write to packed structure for better cache performance
        _dense[_count] = new SparseSetEntry<T>(entity, value);
        
        // Update sparse array to point to dense index
        _sparse[entity.Id] = _count;
        
        _count++;
    }

    /// <summary>
    /// Tries to add a component for the specified entity. Returns false if entity already has this component.
    /// </summary>
    public bool TryAdd(Entity entity, T value)
    {
        if (entity.Id > _maxEntityId || Has(entity))
            return false;

        EnsureCapacity(_count + 1);
        
        // Single write to packed structure
        _dense[_count] = new SparseSetEntry<T>(entity, value);
        
        // Update sparse array to point to dense index
        _sparse[entity.Id] = _count;
        
        _count++;
        return true;
    }

    /// <summary>
    /// Removes the component for the specified entity using swap-with-last algorithm.
    /// Optimized for cache performance with single memory copy.
    /// </summary>
    public void Remove(Entity entity)
    {
        if (entity.Id > _maxEntityId || !Has(entity))
            return;

        var deletingIdx = _sparse[entity.Id];
        var lastIndex = _count - 1;

        if (lastIndex != deletingIdx)
        {
            // Single copy operation for better cache performance
            _dense[deletingIdx] = _dense[lastIndex];
            
            // Update sparse array for the moved entity
            var movedEntityId = _dense[deletingIdx].Entity.Id;
            _sparse[movedEntityId] = deletingIdx;
        }

        // Mark entity as removed in sparse array
        _sparse[entity.Id] = -1;
        _count--;
    }

    /// <summary>
    /// Removes all components from the sparse set.
    /// </summary>
    public void Clear()
    {
        _count = 0;
        
        // Fast clear using optimized Array.Fill
        Array.Fill(_sparse, -1);
        
        // Clear references to help GC - only clear the used portion
        if (_count > 0)
        {
            Array.Clear(_dense, 0, _count);
        }
    }

    /// <summary>
    /// Checks if the specified entity has this component type. Optimized O(1) operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(Entity entity)
    {
        // Branchless optimization - single comparison with bounds check
        var id = entity.Id;
        return id <= _maxEntityId && 
               (uint)_sparse[id] < (uint)_count; // Unsigned comparison handles -1 case
    }
    
    /// <summary>
    /// Gets a read-only span of all components for maximum cache-friendly iteration.
    /// Note: This creates a new array for safety since we can't guarantee stride safety with generic T
    /// </summary>
    public ReadOnlySpan<T> GetAll()
    {
        // For safety with generic T, create a separate array
        // This is still much faster than the old dictionary approach
        var components = new T[_count];
        for (int i = 0; i < _count; i++)
        {
            components[i] = _dense[i].Component;
        }
        return new ReadOnlySpan<T>(components);
    }

    /// <summary>
    /// Gets a read-only span of all entities for efficient entity iteration.
    /// </summary>
    public ReadOnlySpan<Entity> GetAllEntities()
    {
        // Create a separate array for safety
        var entities = new Entity[_count];
        for (int i = 0; i < _count; i++)
        {
            entities[i] = _dense[i].Entity;
        }
        return new ReadOnlySpan<Entity>(entities);
    }

    /// <summary>
    /// Gets both entity and component for efficient iteration with both values.
    /// </summary>
    public ReadOnlySpan<SparseSetEntry<T>> GetAllPairs()
    {
        return new ReadOnlySpan<SparseSetEntry<T>>(_dense, 0, _count);
    }

    /// <summary>
    /// Gets the component for the specified entity. Throws if entity doesn't have this component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(Entity entity)
    {
        if (entity.Id > _maxEntityId)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");
            
        var index = _sparse[entity.Id];
        if (index < 0 || index >= _count)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");
            
        return _dense[index].Component;
    }
    
    /// <summary>
    /// Tries to get the component for the specified entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(Entity entity, out T component)
    {
        if (entity.Id <= _maxEntityId)
        {
            var index = _sparse[entity.Id];
            if ((uint)index < (uint)_count) // Unsigned comparison handles -1 case
            {
                component = _dense[index].Component;
                return true;
            }
        }
        
        component = default;
        return false;
    }

    /// <summary>
    /// Gets a reference to the component for efficient in-place updates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(Entity entity)
    {
        if (entity.Id > _maxEntityId)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");
            
        var index = _sparse[entity.Id];
        if (index < 0 || index >= _count)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");
            
        return ref _dense[index].Component;
    }

    /// <summary>
    /// Gets the maximum entity ID this sparse set can handle
    /// </summary>
    public uint MaxEntityId => _maxEntityId;
    
    /// <summary>
    /// Gets the current count of entities in the set
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets the current capacity of the dense array
    /// </summary>
    public int Capacity => _capacity;

    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _capacity)
            return;

        // Calculate new capacity with alignment for better cache performance
        var newCapacity = Math.Max(_capacity + _growCapacity, _capacity * 2);
        
        // Ensure we meet the required capacity
        if (newCapacity < requiredCapacity)
            newCapacity = requiredCapacity;
            
        // Align to page boundaries for optimal memory layout
        newCapacity = AlignToPage(newCapacity);

        // Create new array and copy existing data
        var newDense = new SparseSetEntry<T>[newCapacity];
        
        // Copy only the used portion using optimized array copy
        if (_count > 0)
        {
            Array.Copy(_dense, 0, newDense, 0, _count);
        }
        
        _dense = newDense;
        _capacity = newCapacity;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateEntityId(uint entityId)
    {
        if (entityId > _maxEntityId)
        {
            throw new ArgumentException($"Entity ID {entityId} exceeds maximum allowed ID of {_maxEntityId}");
        }
    }

    /// <summary>
    /// Aligns capacity to reasonable boundaries for optimal memory layout
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AlignToPage(int capacity)
    {
        // Simple alignment to avoid complex size calculations with generics
        // Align to multiples of 64 for good cache performance
        const int alignment = 64;
        return ((capacity + alignment - 1) / alignment) * alignment;
    }
}