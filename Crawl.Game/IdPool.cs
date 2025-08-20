using Crawl.Game.Exceptions;

namespace Crawl.Game;

public static class GameObjectIdPool
{
    private static uint _nextId = 100;
    private static readonly Lock Lock = new();

    public static uint NewId()
    {
        lock (Lock)
        {
            //uint.MaxValue is reserved as a 'null' id.
            return _nextId != uint.MaxValue
                ? _nextId++
                : throw new IdPoolExhaustedException("available GameObject ID pool exhausted.");
        }
    }
}

public static class ComponentIdPool
{
    private static uint _nextId;
    private static readonly Lock Lock = new();

    public static uint NewId()
    {
        lock (Lock)
        {
            //uint.MaxValue is reserved as a 'null' id.
            return _nextId != uint.MaxValue
                ? _nextId++
                : throw new IdPoolExhaustedException("available Component ID pool exhausted.");
        }
    }
}