using Crawl.ECS.Component;

namespace Crawl.Game;

/// <summary>
///     Thread-safe ID pool that generates sequential uint IDs.
///     uint.MaxValue is reserved as a 'null' or invalid ID.
/// </summary>
public class IdPool(uint startId = 0)
{
    private readonly Lock _lock = new();
    private uint _nextId = startId;

    public uint NewId()
    {
        lock (_lock)
        {
            return _nextId != uint.MaxValue
                ? _nextId++
                : throw new IdPoolExhaustedException($"ID pool exhausted at {uint.MaxValue}");
        }
    }
}

// Convenience static pools for backward compatibility and common use cases
public static class GameObjectIdPool
{
    private static readonly IdPool Pool = new(100);

    public static uint NewId()
    {
        return Pool.NewId();
    }
}

public static class ComponentIdPool
{
    private static readonly IdPool Pool = new();

    public static uint NewId()
    {
        return Pool.NewId();
    }
}