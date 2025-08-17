namespace Crawl.Renderer._2D.Tile;



public readonly record struct Tile(uint TileId, float TraversalSpeed)
{
    /*
     * Properties it needs
     * 'BlockageType' (3 values)
     *      - Floor (floor, can move over, see over, shoot over),
     *      - Half Wall (cant move through, can see over, can shoot over),
     *      - Full Wall (cant move through, cant see over, cant attack over)
     * 
     * 'LightLevel' (how much 'lit' the tile is by default, will be used alongside lighting system) 8 values
     *      - 0 (Not lit, pitch black)
     *      - 1 
     *      - 2
     *      ...
     *      - 7 (Fully light)
     * 'CeilingHeight' (height of ceiling relative to floor, ie CeilingHeight = 0 means 'No Ceiling' CeilingHeight = 1 means ceiling is 1 unit above the floor etc)
     *  - 0..7
     * 'MaterialType' (logical material types for game logic, see MaterialType enum.
     * 
     *
     * ''
     * 
     * 
     */
    public float TraversalSpeed { get; } = TraversalSpeed;

    // IsTraversable returns true if the tile can be traversed through.
    public bool IsTraversable()
    {
        return TraversalSpeed > 0;
    }

    public override string ToString()
    {
        return $"{TileId} (TraversalSpeedMod {TraversalSpeed})";
    }

    public static readonly Tile Empty = new(0, 0);
}

public enum MaterialType : byte
{
    // Logical materials
    Stone = 0,      // Hard, durable
    Wood = 1,       // Flammable, softer
    Metal = 2,      // Conductive, strong
    Ice = 3,        // Slippery, meltable
    Flesh = 4,      // Organic, healing
    Fluid = 5,       // Swim-able, can drown
    Fire = 6        // Burns, ignites flammables 
    // NOT texture IDs!
}