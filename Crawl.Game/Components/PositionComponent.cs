using System.Numerics;
using Crawl.ECS.Component;

namespace Crawl.Game.Components;

public record struct PositionComponent(Vector3 Position) : IComponent
{
    public Vector3 Position { get; private set; } = Position;


    public void MoveTo(Vector3 position)
    {
        Position = position;
    }


    public void MoveBy(float deltaX, float deltaY, float deltaZ = 0)
    {
        Position = new Vector3(Position.X + deltaX, Position.Y + deltaY, Position.Z + deltaZ);
    }

    public float DistanceTo(PositionComponent other)
    {
        return Vector3.Distance(Position, other.Position);
    }
}