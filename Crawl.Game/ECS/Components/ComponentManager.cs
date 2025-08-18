namespace Crawl.ECS.Components;



public class ComponentManager
{
    private readonly IComponentStore[] _stores = new IComponentStore[Enum.GetValues<ComponentType>().Length];

    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        var store = GetOrCreateStore<T>(component.ComponentType);
        store.Add(entity, component);
    }

    private IComponentStore GetOrCreateStore<T>(ComponentType type) where T : struct, IComponent
    {
        return _stores[(uint)type];
    }
}