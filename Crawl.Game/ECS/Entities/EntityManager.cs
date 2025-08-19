namespace Crawl.ECS.Entities;

public class EntityPoolExhaustedException : Exception
{
    public EntityPoolExhaustedException()
    {
    }

    public EntityPoolExhaustedException(string message) : base(message)
    {
    }

    public EntityPoolExhaustedException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class EntityManager
{
    private readonly HashSet<Entity> _entities = [];
    private uint _nextId = 1;

    public int Count => _entities.Count;

    public Entity Create()
    {
        var entity = new Entity(GetId());
        _entities.Add(entity);
        return entity;
    }

    public Entity[] Create(uint quantity)
    {
        var ids = GetId(quantity);
        var entities = new Entity[quantity];

        for (var i = 0; i < entities.Length; i++) entities[i] = new Entity(ids[i]);
        _entities.UnionWith(entities);

        return entities;
    }

    public bool Remove(Entity entity)
    {
        return _entities.Remove(entity);
    }

    public bool Remove(Entity[] entities)
    {
        var originalCount = _entities.Count;
        _entities.ExceptWith(entities);
        return _entities.Count < originalCount;
    }

    public bool Contains(Entity entity)
    {
        return _entities.Contains(entity);
    }

    private uint GetId()
    {
        return _nextId == uint.MaxValue
            ? throw new EntityPoolExhaustedException("entity pool is exhausted")
            : _nextId++;
    }

    private uint[] GetId(uint quantity)
    {
        if (uint.MaxValue - _nextId < quantity)
            throw new EntityPoolExhaustedException("entity pool is exhausted");

        var ids = new uint[quantity];
        for (var i = 0; i < ids.Length; i++) ids[i] = _nextId++;
        return ids;
    }
}