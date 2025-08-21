using Crawl.ECS.Exception;

namespace Crawl.ECS.System;

public readonly record struct StageId(string Id)
{
    public override string ToString()
    {
        return Id;
    }
}

public class Stage
{
    private const int DefaultPriority = -1;

    private readonly List<System> _orderedSystems = [];

    private readonly HashSet<System> _systems = [];

    private bool _needsReorder;

    public void Add(System system, int priority = DefaultPriority)
    {
        system.SetPriority(priority);
        if (!_systems.Add(system))
            throw new ConflictException($"system of type {system.GetType()} already exists");

        _orderedSystems.Add(system);
        _needsReorder = true;
    }

    public void Execute(World world)
    {
        if (_needsReorder)
        {
            _orderedSystems.Sort((s1, s2) => s1.Priority.CompareTo(s2.Priority));
            _needsReorder = false;
        }

        foreach (var system in _orderedSystems.Where(system => system.ShouldExecute(world))) system.Execute(world);
    }
}