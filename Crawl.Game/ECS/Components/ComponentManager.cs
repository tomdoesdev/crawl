using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Crawl.DataStructures;
using Crawl.ECS.Entities;
using Crawl.Exceptions;

namespace Crawl.ECS.Components;

public class ComponentManager
{
    private const int PageSize = 1024;

    //Statics for application uniqueness
    private static readonly Dictionary<Type, int> ComponentTypeIds = new();
    private static readonly Dictionary<int, Type> IdToComponentType = new(); // Reverse lookup
    private static int _nextComponentId;
    private static readonly Lock Lock = new(); // Thread safety


    private readonly Dictionary<int, Bitset[]> _componentPages = new();
    private readonly Dictionary<Type, IComponentStore> _stores = new();


    public int Count => _stores.Values.Sum(store => store.Count);


    public T Get<T>(Entity entity) where T : struct, IComponent
    {
        return !_stores.TryGetValue(typeof(T), out var store)
            ? throw new ComponentNotFoundException($"no {typeof(T)} component for entity {entity}")
            : store.Get<T>(entity);
    }

    public bool TryGet<T>(Entity entity, out T component) where T : struct, IComponent
    {
        if (!_stores.TryGetValue(typeof(T), out var store) || !store.TryGet(entity, out var c))
        {
            component = default;
            return false;
        }

        component = (T)c;
        return true;
    }

    public void Add<T>(Entity entity, T component) where T : struct, IComponent
    {
        var storage = GetOrCreateStorage<T>();
        storage.Add(entity, component);

        var componentId = GetComponentId<T>();
        var (pageIndex, entityIndex) = GetPageCoords(entity.Id);

        if (!_componentPages.TryGetValue(pageIndex, out var page))
        {
            page = new Bitset[PageSize];
            _componentPages[pageIndex] = page;
        }

        page[entityIndex].SetBit(componentId);
    }

    public void Add<T>(Entity[] entities, T component) where T : struct, IComponent
    {
        if (entities.Length == 0) return;

        var storage = GetStorage<T>();
        storage.Add(entities, component);

        var componentType = typeof(T);

        foreach (var entity in entities)
        {
            ref var componentTypes = ref CollectionsMarshal.GetValueRefOrAddDefault(
                _entityComponents, entity, out var exists);

            if (!exists || componentTypes == null)
                componentTypes = [];

            componentTypes.Add(componentType);
        }
    }

    public void RemoveAll(Entity entity)
    {
        if (!_entityComponents.TryGetValue(entity, out var componentTypes))
            return;

        foreach (var componentType in componentTypes)
            if (_stores.TryGetValue(componentType, out var store))
                store.Remove(entity);

        _entityComponents.Remove(entity);
    }

    /// <summary>
    ///     Converts an entity ID into page coordinates (pageIndex, entityIndex)
    /// </summary>
    /// <param name="entityId">The entity ID to convert</param>
    /// <returns>Tuple containing (pageIndex, entityIndexWithinPage)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int pageIndex, int entityIndex) GetPageCoords(uint entityId)
    {
        var pageIndex = (int)(entityId / PageSize);
        var entityIndex = (int)(entityId % PageSize);
        return (pageIndex, entityIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetComponentId<T>() where T : struct, IComponent
    {
        var type = typeof(T);

        // Fast path: ID already exists
        if (ComponentTypeIds.TryGetValue(type, out var existingId))
            return existingId;

        // Slow path: Assign new ID (thread-safe)
        lock (Lock)
        {
            // Double-check in case another thread assigned it
            if (ComponentTypeIds.TryGetValue(type, out existingId))
                return existingId;

            var newId = _nextComponentId++;
            ComponentTypeIds[type] = newId;
            IdToComponentType[newId] = type; // For reverse lookup

            return newId;
        }
    }

    private static Type GetComponentTypeFromId(int componentId)
    {
        return IdToComponentType.TryGetValue(componentId, out var type)
            ? type
            : throw new ArgumentException($"Unknown component ID: {componentId}");
    }

    private ComponentStore<T> GetStorage<T>() where T : struct, IComponent
    {
        if (_stores.TryGetValue(typeof(T), out var storage))
            return (ComponentStore<T>)storage;

        throw new StorageNotFoundException($"no component storage for type {typeof(T)}");
    }

    private ComponentStore<T> GetOrCreateStorage<T>() where T : struct, IComponent
    {
        if (_stores.TryGetValue(typeof(T), out var storage))
            return (ComponentStore<T>)storage;

        var newStorage = new ComponentStore<T>();
        _stores[typeof(T)] = newStorage;
        return newStorage;
    }
}