using System.Diagnostics.CodeAnalysis;
using Crawl.Game.ECS;
using Crawl.Game.Exceptions;

namespace Crawl.Game.DataStructures;

using EntityId = uint;

public struct SparseSetEntry<T>(EntityId entity, T component)
    where T : struct, IComponent
{
    public readonly EntityId Entity = entity;
    public readonly T Component = component;
}

[SuppressMessage("ReSharper", "UseSymbolAlias")]
public class SparseSet<T> where T : struct, IComponent
{
    private const uint PageSize = 1024;
    private const uint PageMask = PageSize - 1;
    private const int EmptyIndex = -1;

    private readonly int _growCapacity;
    private readonly uint _pageShift = (uint)Math.Log2(PageSize);

    private SparseSetEntry<T>[] _dense;
    private int?[]?[] _sparse;

    public SparseSet(EntityId maxEntityId = 10_000, int initialCapacity = 1000, int growCapacity = 1000)
    {
        var pageCount = (maxEntityId + PageSize) / PageSize;
        _sparse = new int?[pageCount][];
        _dense = new SparseSetEntry<T>[initialCapacity];

        _growCapacity = growCapacity;

        MaxEntityId = maxEntityId;
        Count = 0;
        Capacity = initialCapacity;
    }

    public int Capacity { get; private set; }

    public int Count { get; private set; }

    public uint MaxEntityId { get; }


    public bool Add(EntityId entity, T component)
    {
        ValidateEntityId(entity);

        if (Contains(entity))
            throw new ConflictException($"entity with id {entity} already exists");

        EnsureCapacity(Count + 1);

        _dense[Count] = new SparseSetEntry<T>(entity, component);

        SetSparseIndex(entity, Count);

        Count++;

        return false;
    }

    public SparseSetEntry<T> Get(EntityId entity)
    {
        ValidateEntityId(entity);

        var idx = GetSparseIndex(entity);

        return idx == EmptyIndex || idx >= Count
            ? throw new ComponentNotFoundException($"no component of type {typeof(T)} for entity {entity}")
            : _dense[idx];
    }

    public bool Remove(EntityId entity)
    {
        if (entity > MaxEntityId)
            return false;

        var deletingIdx = GetSparseIndex(entity);

        if (deletingIdx < 0 || deletingIdx >= Count)
            return false;

        var lastIdx = Count - 1;

        // Short circuit case when deleting last element
        if (lastIdx == deletingIdx)
        {
            SetSparseIndex(entity, EmptyIndex);
            Count = lastIdx;
            return true;
        }

        ref var deletingEntry = ref _dense[deletingIdx];
        ref var lastEntry = ref _dense[lastIdx];

        var movedEntity = lastEntry.Entity;

        deletingEntry = lastEntry;

        SetSparseIndex(movedEntity, deletingIdx);
        SetSparseIndex(entity, EmptyIndex);
        Count = lastIdx;

        return true;
    }

    public bool Contains(EntityId entity)
    {
        if (entity > MaxEntityId)
            return false;

        var index = GetSparseIndex(entity);

        return (uint)index < (uint)Count;
    }

    public ReadOnlySpan<SparseSetEntry<T>> GetAll()
    {
        return Count == 0
            ? ReadOnlySpan<SparseSetEntry<T>>.Empty
            : new ReadOnlySpan<SparseSetEntry<T>>(_dense, 0, Count);
    }

    #region Internal

    private int GetSparseIndex(EntityId entityId)
    {
        var pageIndex = entityId >> (int)_pageShift;
        var page = _sparse[pageIndex];

        if (page == null)
            return EmptyIndex;

        var elementIndex = entityId & PageMask;
        return page[elementIndex] ?? EmptyIndex;
    }

    private void SetSparseIndex(uint entityId, int index)
    {
        var pageIndex = entityId >> (int)_pageShift;
        var elementIndex = entityId & PageMask;

        ref var page = ref _sparse[pageIndex];

        if (index == EmptyIndex)
        {
            if (page != null) page[elementIndex] = null;
            return;
        }

        page ??= new int?[PageSize];

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

    private static int AlignToCache(int capacity)
    {
        const int alignment = 16;
        return (capacity + alignment - 1) / alignment * alignment;
    }

    private void ValidateEntityId(EntityId entity)
    {
        if (entity > MaxEntityId || entity == uint.MaxValue)
            throw new InvalidEntityException($"entity id {entity} is invalid. Max {MaxEntityId}");
    }

    #endregion
}