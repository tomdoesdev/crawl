using System.ComponentModel;

namespace Crawl.ECS.Components;

public interface IComponent
{
    ComponentType ComponentType { get; }
}

public enum ComponentType : uint
{
    Position,
    Velocity,
    Health,
    Sprite2D
}