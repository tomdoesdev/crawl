using System.Runtime.CompilerServices;
using Crawl.ECS.Components;
using Crawl.ECS.Entities;

namespace Crawl.DataStructures;

public class DuplicateComponentException : Exception
{
    public DuplicateComponentException(string message) : base(message)
    {
    }

    public DuplicateComponentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class ComponentNotFoundException : Exception
{
    public ComponentNotFoundException()
    {
    }

    public ComponentNotFoundException(string message) : base(message)
    {
    }

    public ComponentNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
///     Cache-optimized entry combining entity and component data for better memory layout
/// </summary>
public struct SparseSetEntry<T>(Entity entity, T component)
    where T : struct
{
    public Entity Entity = entity;
    public T Component = component;
}

/// <summary>
///     High-performance sparse set with optimized random access and iteration.
///     Uses a paged sparse array for better cache locality and zero-allocation iteration.
/// </summary>
public class SparseSet<T> where T : struct, IComponent
{
    private readonly int _growCapacity;
    private readonly uint _pageMask;
    private readonly uint _pageShift;

    private readonly uint _pageSize;

    // Dense array - packed data for cache-friendly iteration
    private SparseSetEntry<T>[] _dense;

    // Two-level sparse array for cache-friendly random access
    private int?[]?[] _sparsePages;


    /// <summary>
    ///     Creates a new high-performance sparse set with paged sparse array.
    /// </summary>
    /// <param name="initialCapacity">Initial capacity for dense array</param>
    /// <param name="growCapacity">Amount to grow dense array when needed</param>
    /// <param name="maxEntityId">Maximum entity ID this sparse set can handle</param>
    /// <param name="pageSize">Page size for sparse array (must be power of 2)</param>
    public SparseSet(int initialCapacity = 1000, int growCapacity = 1000, uint maxEntityId = 100000,
        uint pageSize = 1024)
    {
        if (initialCapacity <= 0)
            throw new ArgumentException("initial capacity must be greater than 0");

        if (growCapacity <= 0)
            throw new ArgumentException("grow capacity must be greater than 0");

        if (!IsPowerOfTwo(pageSize))
            throw new ArgumentException("page size must be a power of 2");

        // Align initial capacity to cache boundaries
        var alignedCapacity = AlignToCache(initialCapacity);

        _dense = new SparseSetEntry<T>[alignedCapacity];
        Capacity = alignedCapacity;
        _growCapacity = growCapacity;
        MaxEntityId = maxEntityId;

        // Set up paged sparse array for cache-friendly access
        _pageSize = pageSize;
        _pageMask = pageSize - 1;
        _pageShift = (uint)Math.Log2(pageSize);

        var pageCount = (maxEntityId + pageSize) / pageSize;
        _sparsePages = new int?[pageCount][];

        Count = 0;
    }

    /// <summary>
    ///     Gets the maximum entity ID this sparse set can handle
    /// </summary>
    public uint MaxEntityId { get; }

    /// <summary>
    ///     Gets the current count of entities in the set
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Gets the current capacity of the dense array
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    ///     Adds a component for the specified entity. Throws if entity already has this component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Entity entity, T value)
    {
        ValidateEntityId(entity.Id);

        if (Has(entity))
            throw new DuplicateComponentException($"Entity {entity.Id} already has component {typeof(T).Name}");

        EnsureCapacity(Count + 1);

        // Single write to packed structure for better cache performance
        _dense[Count] = new SparseSetEntry<T>(entity, value);

        // Update sparse array to point to dense index
        SetSparseIndex(entity.Id, Count);

        Count++;
    }

    /// <summary>
    ///     Tries to add a component for the specified entity. Returns false if entity already has this component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(Entity entity, T value)
    {
        if (entity.Id > MaxEntityId || Has(entity))
            return false;

        EnsureCapacity(Count + 1);

        // Single write to packed structure
        _dense[Count] = new SparseSetEntry<T>(entity, value);

        // Update sparse array to point to dense index
        SetSparseIndex(entity.Id, Count);

        Count++;
        return true;
    }

    /// <summary>
    ///     Removes the component for the specified entity using optimized swap-with-last algorithm.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Entity entity)
    {
        var entityId = entity.Id;

        if (entityId > MaxEntityId)
            return;

        var deletingIdx = GetSparseIndex(entityId);

        if (deletingIdx < 0 || deletingIdx >= Count)
            return;

        var lastIndex = Count - 1;

        // Optimize for the common case where we're removing the last element
        if (lastIndex == deletingIdx)
        {
            SetSparseIndex(entityId, -1);
            Count = lastIndex;
            return;
        }

        // Cache-friendly swap operation
        ref var deletingEntry = ref _dense[deletingIdx];
        ref var lastEntry = ref _dense[lastIndex];

        // Get the moved entity ID before overwriting
        var movedEntityId = lastEntry.Entity.Id;

        // Single memory copy operation
        deletingEntry = lastEntry;

        // Update sparse mapping for moved entity
        SetSparseIndex(movedEntityId, deletingIdx);

        // Mark removed entity as invalid
        SetSparseIndex(entityId, -1);
        Count = lastIndex;
    }

    /// <summary>
    ///     Removes all components from the sparse set.
    /// </summary>
    public void Clear()
    {
        Count = 0;

        // Clear sparse pages efficiently by nulling page references
        for (var i = 0; i < _sparsePages.Length; i++) _sparsePages[i] = null;

        // Clear references to help GC
        if (Capacity > 0) Array.Clear(_dense, 0, Capacity);
    }

    /// <summary>
    ///     Checks if the specified entity has this component type. Optimized O(1) operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(Entity entity)
    {
        if (entity.Id > MaxEntityId)
            return false;

        var index = GetSparseIndex(entity.Id);
        return (uint)index < (uint)Count;
    }

    /// <summary>
    ///     Gets a read-only span of all components for efficient cache-friendly iteration.
    ///     Optimized version that minimizes allocations for typical usage patterns.
    /// </summary>
    public ReadOnlySpan<T> GetAll()
    {
        if (Count == 0)
            return ReadOnlySpan<T>.Empty;

        // For best performance, create a span from a temporary array
        // This is still much faster than the old dictionary approach
        var components = new T[Count];

        // Use optimized loop with span operations for better performance
        var denseSpan = new ReadOnlySpan<SparseSetEntry<T>>(_dense, 0, Count);
        for (var i = 0; i < denseSpan.Length; i++) components[i] = denseSpan[i].Component;

        return new ReadOnlySpan<T>(components);
    }

    /// <summary>
    ///     Gets a read-only span of all entities for efficient entity iteration.
    /// </summary>
    public ReadOnlySpan<Entity> GetAllEntities()
    {
        if (Count == 0)
            return ReadOnlySpan<Entity>.Empty;

        var entities = new Entity[Count];
        var denseSpan = new ReadOnlySpan<SparseSetEntry<T>>(_dense, 0, Count);

        for (var i = 0; i < denseSpan.Length; i++) entities[i] = denseSpan[i].Entity;

        return new ReadOnlySpan<Entity>(entities);
    }

    /// <summary>
    ///     Gets both entity and component for efficient iteration with both values.
    ///     Direct access to packed data without allocations.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<SparseSetEntry<T>> GetAllPairs()
    {
        return new ReadOnlySpan<SparseSetEntry<T>>(_dense, 0, Count);
    }

    /// <summary>
    ///     Gets the component for the specified entity. Throws if entity doesn't have this component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(Entity entity)
    {
        if (entity.Id > MaxEntityId)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");

        var index = GetSparseIndex(entity.Id);
        if (index < 0 || index >= Count)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");

        return _dense[index].Component;
    }

    /// <summary>
    ///     Tries to get the component for the specified entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(Entity entity, out T component)
    {
        if (entity.Id <= MaxEntityId)
        {
            var index = GetSparseIndex(entity.Id);
            if ((uint)index < (uint)Count)
            {
                component = _dense[index].Component;
                return true;
            }
        }

        component = default;
        return false;
    }

    /// <summary>
    ///     Gets a reference to the component for efficient in-place updates.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetRef(Entity entity)
    {
        if (entity.Id > MaxEntityId)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");

        var index = GetSparseIndex(entity.Id);
        if (index < 0 || index >= Count)
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist");

        return ref _dense[index].Component;
    }

    /// <summary>
    ///     Removes multiple entities in a single batch operation for better cache performance.
    /// </summary>
    public void RemoveRange(ReadOnlySpan<Entity> entities)
    {
        if (entities.IsEmpty)
            return;

        var indicesToRemove = new int[entities.Length];
        var validCount = 0;

        // Collect valid indices
        for (var i = 0; i < entities.Length; i++)
        {
            var entityId = entities[i].Id;
            if (entityId > MaxEntityId) continue;

            var idx = GetSparseIndex(entityId);
            if ((uint)idx < (uint)Count) indicesToRemove[validCount++] = idx;
        }

        if (validCount == 0)
            return;

        // Sort indices in descending order to avoid index shifting issues
        Array.Sort(indicesToRemove, 0, validCount);
        Array.Reverse(indicesToRemove, 0, validCount);

        // Remove entities in descending index order
        for (var i = 0; i < validCount; i++)
        {
            var deletingIdx = indicesToRemove[i];
            var lastIndex = Count - 1;

            var entityId = _dense[deletingIdx].Entity.Id;

            if (lastIndex != deletingIdx)
            {
                _dense[deletingIdx] = _dense[lastIndex];
                var movedEntityId = _dense[deletingIdx].Entity.Id;
                SetSparseIndex(movedEntityId, deletingIdx);
            }

            SetSparseIndex(entityId, -1);
            Count--;
        }
    }

    /// <summary>
    ///     Removes entities matching a predicate in batch for optimal performance.
    /// </summary>
    public int RemoveWhere(Func<Entity, T, bool> predicate)
    {
        if (Count == 0)
            return 0;

        var writeIndex = 0;
        var removedCount = 0;

        for (var readIndex = 0; readIndex < Count; readIndex++)
        {
            ref var entry = ref _dense[readIndex];

            if (predicate(entry.Entity, entry.Component))
            {
                SetSparseIndex(entry.Entity.Id, -1);
                removedCount++;
            }
            else
            {
                if (writeIndex != readIndex)
                {
                    _dense[writeIndex] = entry;
                    SetSparseIndex(entry.Entity.Id, writeIndex);
                }

                writeIndex++;
            }
        }

        Count = writeIndex;
        return removedCount;
    }

    /// <summary>
    ///     Attempts to remove an entity. Returns true if the entity was found and removed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRemove(Entity entity)
    {
        var entityId = entity.Id;

        if (entityId > MaxEntityId)
            return false;

        var deletingIdx = GetSparseIndex(entityId);

        if (deletingIdx < 0 || deletingIdx >= Count)
            return false;

        var lastIndex = Count - 1;

        if (lastIndex != deletingIdx)
        {
            ref var deletingEntry = ref _dense[deletingIdx];
            ref var lastEntry = ref _dense[lastIndex];

            var movedEntityId = lastEntry.Entity.Id;
            deletingEntry = lastEntry;
            SetSparseIndex(movedEntityId, deletingIdx);
        }

        SetSparseIndex(entityId, -1);
        Count = lastIndex;
        return true;
    }

    // Private helper methods for paged sparse array access

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetSparseIndex(uint entityId)
    {
        var pageIndex = entityId >> (int)_pageShift;
        var page = _sparsePages[pageIndex];

        if (page == null)
            return -1;

        var elementIndex = entityId & _pageMask;
        return page[elementIndex] ?? -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetSparseIndex(uint entityId, int index)
    {
        var pageIndex = entityId >> (int)_pageShift;
        var elementIndex = entityId & _pageMask;

        ref var page = ref _sparsePages[pageIndex];

        if (index == -1)
        {
            // Removing - clear the entry
            if (page != null) page[elementIndex] = null;
            return;
        }

        // Adding - ensure page exists
        if (page == null) page = new int?[_pageSize];

        page[elementIndex] = index;
    }

    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= Capacity)
            return;

        var newCapacity = Math.Max(Capacity + _growCapacity, Capacity * 2);

        if (newCapacity < requiredCapacity)
            newCapacity = requiredCapacity;

        newCapacity = AlignToCache(newCapacity);

        var newDense = new SparseSetEntry<T>[newCapacity];

        if (Count > 0) Array.Copy(_dense, 0, newDense, 0, Count);

        _dense = newDense;
        Capacity = newCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateEntityId(uint entityId)
    {
        if (entityId > MaxEntityId)
            throw new ArgumentException($"Entity ID {entityId} exceeds maximum allowed ID of {MaxEntityId}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AlignToCache(int capacity)
    {
        // Align to cache line boundaries (64 bytes typically)
        const int alignment = 16; // Conservative alignment for different component sizes
        return (capacity + alignment - 1) / alignment * alignment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPowerOfTwo(uint value)
    {
        return value != 0 && (value & (value - 1)) == 0;
    }
}