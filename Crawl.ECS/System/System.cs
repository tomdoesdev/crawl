namespace Crawl.ECS.System;

public abstract class System
{
    public int Priority { get; private set; }


    public abstract void Execute(World world);

    public bool ShouldExecute(World world)
    {
        return true;
    }

    public void SetPriority(int priority)
    {
        Priority = priority;
    }

    public sealed override int GetHashCode()
    {
        return GetType().GetHashCode();
    }
}