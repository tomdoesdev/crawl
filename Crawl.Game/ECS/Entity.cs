namespace Crawl.ECS;
public readonly record struct Entity(uint Id)
{
    public readonly uint Id = Id;
}

public enum SentinelEntities : uint
{
    Null = 0,
}

public class EntityManager(uint maxReleasedIdCapacity = EntityManager.DefaultMaxReleaseCapacity)
{
    private const uint DefaultMaxReleaseCapacity = 10000;
    
    private uint _currentId = 1; // ids start at 1 as 0 is reserved as a sentinel value.
    private readonly Pool<uint> _idPool = new(maxReleasedIdCapacity);

    public Entity Create()
    {
        if (!_idPool.TryPluck(out var id))
            return _currentId != uint.MaxValue
                ? new Entity(_currentId++)
                : throw new InvalidOperationException("entity id pool exhausted");
        
        return new Entity(id);

    }

    public void Remove(Entity entity)
    {
        if (entity.Id == (uint)SentinelEntities.Null)
        {
            return;
        }
        
        
        
        
        _idPool.Add(entity.Id);
        //TODO: Unbind entity components etc here
    }
}
