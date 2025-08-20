using Crawl.DataStructures;
using Crawl.ECS.Entities;

namespace Crawl.ECS.Components;

public interface IComponentStore
{
    int Count { get; }

    /// <summary>
    ///     Adds a component to the store bound to the given entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="component"></param>
    void Add(Entity entity, IComponent component);

    void Add(Entity[] entities, IComponent component);

    /// <summary>
    ///     Adds a component to the store, returning true if sucessful
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    bool TryAdd(Entity entity, IComponent component);

    /// <summary>
    ///     Removes the component associated to the given Entity.
    /// </summary>
    /// <param name="entity"></param>
    void Remove(Entity entity);

    /// <summary>
    ///     Gets the component for the given entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TResult Get<TResult>(Entity entity) where TResult : struct, IComponent;

    /// <summary>
    ///     Gets the value associated with the given Entity
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="component"></param>
    /// <returns> true if it existed, false otherwise </returns>
    bool TryGet(Entity entity, out IComponent component);

    /// <summary>
    ///     Returns whether the store contains a component for given entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>true if entity has component, false if not</returns>
    bool Has(Entity entity);

    /// <summary>
    ///     Returns All components from the store
    /// </summary>
    /// <returns></returns>
    ReadOnlySpan<IComponent> GetAll();
}

public class ComponentStore<T> : IComponentStore where T : struct, IComponent
{
    private readonly SparseSet<T> _store = new();

    public int Count => _store.Count;


    public void Add(Entity entity, IComponent component)
    {
        if (component is not T typedComponent)
            throw new ArgumentException($"Expected component of type {typeof(T)}, got {component.GetType()}");

        _store.Add(entity, typedComponent);
    }

    public void Add(Entity[] entities, IComponent component)
    {
        if (component is not T typedComponent)
            throw new ArgumentException($"Expected component of type {typeof(T)}, got {component.GetType()}");

        _store.Add(entities, typedComponent);
    }

    public bool TryAdd(Entity entity, IComponent component)
    {
        return component is T typedComponent && _store.TryAdd(entity, typedComponent);
    }

    public void Remove(Entity entity)
    {
        _store.Remove(entity);
    }

    public TResult Get<TResult>(Entity entity) where TResult : struct, IComponent
    {
        return (TResult)(IComponent)_store.Get(entity);
    }

    public bool TryGet(Entity entity, out IComponent component)
    {
        if (_store.TryGet(entity, out var typedComponent))
        {
            component = typedComponent;
            return true;
        }

        component = null!;
        return false;
    }

    public bool Has(Entity entity)
    {
        return _store.Has(entity);
    }

    public ReadOnlySpan<IComponent> GetAll()
    {
        var components = _store.GetAll();
        var result = new IComponent[components.Length];

        for (var i = 0; i < components.Length; i++) result[i] = components[i];

        return new ReadOnlySpan<IComponent>(result);
    }
}