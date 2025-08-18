using Crawl.ECS.Entities;

namespace Crawl.ECS.Components;



public class ComponentManager
{
    private readonly Dictionary<ComponentType, IComponentStore> _stores = new();
    private readonly Dictionary<Type, ComponentType> _componentTypes = new();
    
    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        if (!_componentTypes.ContainsKey(component.GetType()))
        {
            _componentTypes[component.GetType()] = component.ComponentType;
        }
        
        var store = GetOrCreateStore<T>(component.ComponentType);
        store.Add(entity, component);
    }

    private IComponentStore GetOrCreateStore<T>(ComponentType type) where T : struct, IComponent
    {
        if (_stores.TryGetValue(type, out var store)) return store;
        store = new ComponentStore<T>();
        _stores[type] = store;

        return store;
    }

    public T GetComponent<T>(Entity entity) where T : struct, IComponent
    {
        if (!_componentTypes.TryGetValue(typeof(T), out var componentType))
        {
            throw new Exception($"Component type {typeof(T)} does not exist");
        }

        return !_stores.TryGetValue(componentType, out var store) ? throw new Exception($"No store for component type {typeof(T)}") : store.Get<T>(entity);
    }
}