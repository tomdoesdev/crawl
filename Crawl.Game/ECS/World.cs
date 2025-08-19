using Crawl.ECS.Components;
using Crawl.ECS.Entities;

namespace Crawl.ECS;

public class World
{
    private readonly Dictionary<Type, IComponentStore> _componentStores = new();
    private readonly EntityManager _entityManager = new();


    public int EntityCount => _entityManager.Count;

    public int ComponentCount =>
        _componentStores.Values.Sum(store => store.Count);


    #region Component

    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        var storage = GetStorage<T>();
        storage.Add(entity, component);
    }

    public T GetComponent<T>(Entity entity) where T : struct, IComponent
    {
        var store = GetStorage<T>();
        return store.Get<T>(entity);
    }

    private ComponentStore<T> GetStorage<T>() where T : struct, IComponent
    {
        if (_componentStores.TryGetValue(typeof(T), out var storage))
            return (ComponentStore<T>)storage;

        var newStorage = new ComponentStore<T>();
        _componentStores[typeof(T)] = newStorage;
        return newStorage;
    }

    #endregion

    #region Entity

    public Entity CreateEntity()
    {
        return _entityManager.Create();
    }

    public Entity[] CreateEntity(uint quantity)
    {
        return _entityManager.Create(quantity);
    }

    public bool DestroyEntity(Entity entity)
    {
        //TODO: DestroyEntity needs to also remove components!
        return _entityManager.Remove(entity);
    }

    public bool DestroyEntity(Entity[] entities)
    {
        //TODO: DestroyEntity needs to also remove components!
        return _entityManager.Remove(entities);
    }

    public bool ContainsEntity(Entity entity)
    {
        return _entityManager.Contains(entity);
    }

    #endregion
}