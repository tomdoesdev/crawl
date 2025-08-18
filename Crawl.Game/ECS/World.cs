namespace Crawl.ECS;

public class World
{
    private readonly Dictionary<Type, object> _componentStorages;

    public World()
    {
        _componentStorages = new Dictionary<Type, object>();
    }

    private SparseSet<T> GetStorage<T>() where T : struct, IComponent
    {
        if (_componentStorages.TryGetValue(typeof(T), out var storage)) return (SparseSet<T>)storage;
        storage = new SparseSet<T>();
        _componentStorages[typeof(T)] = storage;
        return (SparseSet<T>)storage;
    }
}