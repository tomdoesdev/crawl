using OpenTK.Mathematics;

namespace Crawl.Renderer._2D;


public static class Colors
{
    // Dungeon Floor & Walls
    public static readonly Vector3 StoneFloor = new(0.4f, 0.4f, 0.45f);     // Cool gray
    public static readonly Vector3 StoneWall = new(0.3f, 0.25f, 0.2f);      // Dark brown-gray
    public static readonly Vector3 DarkStoneWall = new(0.15f, 0.1f, 0.08f); // Very dark stone

    // Water & Liquids
    public static readonly Vector3 DeepWater = new(0.1f, 0.3f, 0.6f);       // Deep blue
    public static readonly Vector3 ShallowWater = new(0.2f, 0.5f, 0.8f);    // Lighter blue
    public static readonly Vector3 Lava = new(1.0f, 0.3f, 0.1f);            // Bright orange-red

    // Interactive Elements
    public static readonly Vector3 Door = new(0.6f, 0.3f, 0.1f);            // Brown wood
    public static readonly Vector3 LockedDoor = new(0.4f, 0.2f, 0.05f);     // Darker brown
    public static readonly Vector3 Key = new(1.0f, 0.8f, 0.1f);             // Golden yellow

    // Character & Enemies
    public static readonly Vector3 Hero = new(0.2f, 0.7f, 0.2f);            // Bright green
    public static readonly Vector3 Enemy = new(0.8f, 0.2f, 0.2f);           // Red
    public static readonly Vector3 DeadEnemy = new(0.4f, 0.1f, 0.1f);       // Dark red

    // Special Areas
    public static readonly Vector3 StartArea = new(0.5f, 0.8f, 0.5f);       // Light green
    public static readonly Vector3 TreasureArea = new(0.8f, 0.6f, 0.1f);    // Gold
    public static readonly Vector3 DangerArea = new(0.6f, 0.1f, 0.6f);      // Purple

    // Void/Empty
    public static readonly Vector3 Void = new(0.05f, 0.05f, 0.1f);          // Almost black with blue tint
}