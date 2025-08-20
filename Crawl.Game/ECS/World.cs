using Crawl.ECS.Components;
using Crawl.ECS.Entities;

namespace Crawl.ECS;

public class World
{
    private readonly ComponentManager _componentManager = new();
    private readonly EntityManager _entityManager = new();


    public int EntityCount => _entityManager.Count;

    public int ComponentCount => _componentManager.Count;

    #region Component

    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        _componentManager.Add(entity, component);
    }

    public void AddComponent<T>(Entity[] entities, T component) where T : struct, IComponent
    {
        _componentManager.Add(entities, component);
    }

    public T GetComponent<T>(Entity entity) where T : struct, IComponent
    {
        return _componentManager.Get<T>(entity);
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