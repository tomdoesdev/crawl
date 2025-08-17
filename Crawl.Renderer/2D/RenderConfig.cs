namespace Crawl.Renderer._2D;

/// <summary>
/// Configuration constants for 2D rendering
/// </summary>
public static class RenderConfig
{
    // Tile settings
    public const float TileSize = 32.0f;
    
    // Default window dimensions
    public const int DefaultWindowWidth = 800;
    public const int DefaultWindowHeight = 600;
    
    // Shader transformation constants
    // These match the current shader: normalized.x = aPosition.x / 200.0 - 1.0
    public const float WorldToNdcScaleX = 200.0f;
    public const float WorldToNdcScaleY = 150.0f;
    
    // Vertex layout
    public const int PositionComponents = 2;  // x, y
    public const int ColorComponents = 3;     // r, g, b
    public const int VertexComponents = PositionComponents + ColorComponents; // 5 total
    public const int VertexSizeBytes = VertexComponents * sizeof(float);
    
    // Triangles per tile (quad = 2 triangles)
    public const int TrianglesPerTile = 2;
    public const int VerticesPerTriangle = 3;
    public const int VerticesPerTile = TrianglesPerTile * VerticesPerTriangle; // 6
}