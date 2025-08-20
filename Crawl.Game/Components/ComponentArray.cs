using Crawl.Game.ECS;
using Crawl.Game.Exceptions;

namespace Crawl.Game.Components;

public interface IComponentArray
{
    int Count { get; }
    bool HasComponent(uint gameObjectId);
    bool RemoveComponent(uint gameObjectId);
}

/// <summary>
///     Sparse set-based storage for components of type T
///     Provides O(1) access and dense iteration for batch processing
/// </summary>
public class ComponentArray<T> : IComponentArray where T : struct, IComponent
{
    private const int InitialCapacity = 64;
    private const uint InvalidGameObjectId = uint.MaxValue;

    // Dense arrays: packed component data for cache-friendly iteration
    private T[] _dense;

    private uint[] _gameObjectIds; // Which GameObject owns each dense slot

    // Sparse array: maps gameObjectId -> index in dense arrays
    private int[] _sparse;

    public ComponentArray()
    {
        _sparse = new int[InitialCapacity];
        _dense = new T[InitialCapacity];
        _gameObjectIds = new uint[InitialCapacity];

        // Initialize sparse array with "not found" sentinel
        Array.Fill(_sparse, -1);
        Array.Fill(_gameObjectIds, InvalidGameObjectId);
    }

    public int Count { get; private set; }

    /// <summary>
    ///     Check if GameObject has this component type
    /// </summary>
    public bool HasComponent(uint gameObjectId)
    {
        if (gameObjectId >= _sparse.Length) return false;

        var sparseIndex = _sparse[gameObjectId];
        return sparseIndex != -1 &&
               sparseIndex < Count &&
               _gameObjectIds[sparseIndex] == gameObjectId;
    }

    /// <summary>
    ///     Remove component from GameObject using swap-and-pop
    /// </summary>
    public bool RemoveComponent(uint gameObjectId)
    {
        if (!HasComponent(gameObjectId)) return false;

        var indexToRemove = _sparse[gameObjectId];
        var lastIndex = Count - 1;

        if (indexToRemove != lastIndex)
        {
            // Move last element to the removed slot (swap-and-pop)
            _dense[indexToRemove] = _dense[lastIndex];
            var lastGameObjectId = _gameObjectIds[lastIndex];
            _gameObjectIds[indexToRemove] = lastGameObjectId;

            // Update sparse array for the moved element
            _sparse[lastGameObjectId] = indexToRemove;
        }

        // Clear the sparse entry for removed GameObject
        _sparse[gameObjectId] = -1;
        Count--;

        return true;
    }

    /// <summary>
    ///     Add or update component for the given GameObject
    /// </summary>
    public void Add(uint gameObjectId, T component)
    {
        EnsureSparseCapacity(gameObjectId);

        var sparseIndex = _sparse[gameObjectId];

        if (sparseIndex != -1 && sparseIndex < Count && _gameObjectIds[sparseIndex] == gameObjectId)
        {
            // Update existing component
            _dense[sparseIndex] = component;
        }
        else
        {
            // Add new component
            EnsureDenseCapacity();

            _sparse[gameObjectId] = Count;
            _dense[Count] = component;
            _gameObjectIds[Count] = gameObjectId;
            Count++;
        }
    }

    /// <summary>
    ///     Get component by value (creates a copy for structs)
    /// </summary>
    public T Get(uint gameObjectId)
    {
        var index = GetDenseIndex(gameObjectId);
        return _dense[index];
    }

    /// <summary>
    ///     Get component by reference (no copying - modifiable)
    /// </summary>
    public ref T GetRef(uint gameObjectId)
    {
        var index = GetDenseIndex(gameObjectId);
        return ref _dense[index];
    }

    /// <summary>
    ///     Get all components for batch processing (cache-friendly iteration)
    /// </summary>
    public ReadOnlySpan<T> GetComponents()
    {
        return _dense.AsSpan(0, Count);
    }

    /// <summary>
    ///     Get all GameObject IDs that have this component
    /// </summary>
    public ReadOnlySpan<uint> GetGameObjectIds()
    {
        return _gameObjectIds.AsSpan(0, Count);
    }

    private int GetDenseIndex(uint gameObjectId)
    {
        if (!HasComponent(gameObjectId))
            throw new ComponentNotFoundException($"GameObject {gameObjectId} does not have component {typeof(T).Name}");

        return _sparse[gameObjectId];
    }

    private void EnsureSparseCapacity(uint gameObjectId)
    {
        if (gameObjectId >= _sparse.Length)
        {
            var newCapacity = Math.Max((int)gameObjectId + 1, _sparse.Length * 2);
            var newSparse = new int[newCapacity];

            Array.Copy(_sparse, newSparse, _sparse.Length);
            Array.Fill(newSparse, -1, _sparse.Length, newCapacity - _sparse.Length);

            _sparse = newSparse;
        }
    }

    private void EnsureDenseCapacity()
    {
        if (Count >= _dense.Length)
        {
            var newCapacity = _dense.Length * 2;

            Array.Resize(ref _dense, newCapacity);
            Array.Resize(ref _gameObjectIds, newCapacity);

            // Initialize new slots
            Array.Fill(_gameObjectIds, InvalidGameObjectId, Count, newCapacity - Count);
        }
    }
}