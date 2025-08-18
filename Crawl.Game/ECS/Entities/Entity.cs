namespace Crawl.ECS.Entities;
public readonly record struct Entity(uint Id)
{
    public readonly uint Id = Id;
}

public enum SentinelEntities : uint
{
    Null = 0,
}