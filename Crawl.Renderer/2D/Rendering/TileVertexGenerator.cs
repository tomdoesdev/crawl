using Crawl.Renderer._2D.Texture;
using Crawl.Renderer._2D.Tile;
using OpenTK.Mathematics;

namespace Crawl.Renderer._2D.Rendering;

/// <summary>
/// Generates vertex data for tile-based rendering.
/// </summary>
public static class TileVertexGenerator
{
    /// <summary>
    /// Generates vertex data for a 2D tile layout with colors from TextureManager.
    /// </summary>
    /// <param name="tileLayout">2D array of tile IDs</param>
    /// <param name="textureManager">Manager containing color data for tile IDs</param>
    /// <param name="tileSize">Size of each tile in world units</param>
    /// <returns>Array of vertex data (position + color)</returns>
    public static float[] GenerateVertices(uint[,] tileLayout, TextureManager textureManager, float tileSize = RenderConfig.TileSize)
    {
        var rows = tileLayout.GetLength(0);
        var cols = tileLayout.GetLength(1);
        
        // Pre-calculate array size for better performance
        var totalVertices = rows * cols * RenderConfig.VerticesPerTile;
        var totalFloats = totalVertices * RenderConfig.VertexComponents;
        var vertices = new float[totalFloats];
        
        var index = 0;
        
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var x = col * tileSize;
                var y = row * tileSize;
                
                // Get color for this tile
                var tileId = tileLayout[row, col];
                var tileColor = textureManager.Get(tileId).Color;
                
                // Generate quad as two triangles
                AddQuadVertices(vertices, ref index, x, y, tileSize, tileColor);
            }
        }
        
        return vertices;
    }
    
    /// <summary>
    /// Generates vertex data for tiles from a TileMap.
    /// </summary>
    /// <param name="tileMap">TileMap containing tile data</param>
    /// <param name="textureManager">Manager containing color data for tile IDs</param>
    /// <param name="tileSize">Size of each tile in world units</param>
    /// <returns>Array of vertex data (position + color)</returns>
    public static float[] GenerateVertices(TileMap tileMap, TextureManager textureManager, float tileSize = RenderConfig.TileSize)
    {
        var allTiles = tileMap.GetAllTiles().ToArray();
        
        if (allTiles.Length == 0)
            return [];
        
        // Pre-calculate array size
        var totalVertices = allTiles.Length * RenderConfig.VerticesPerTile;
        var totalFloats = totalVertices * RenderConfig.VertexComponents;
        var vertices = new float[totalFloats];
        
        var index = 0;
        
        foreach (var (x, y, tile) in allTiles)
        {
            var worldX = x * tileSize;
            var worldY = y * tileSize;
            
            // Get color for this tile
            var tileColor = textureManager.Get(tile.TileId).Color;
            
            // Generate quad as two triangles
            AddQuadVertices(vertices, ref index, worldX, worldY, tileSize, tileColor);
        }
        
        return vertices;
    }
    
    private static void AddQuadVertices(float[] vertices, ref int index, float x, float y, float tileSize, Vector3 color)
    {
        // Triangle 1: bottom-left, bottom-right, top-right
        AddVertex(vertices, ref index, x, y, color);                          // Bottom-left
        AddVertex(vertices, ref index, x + tileSize, y, color);               // Bottom-right
        AddVertex(vertices, ref index, x + tileSize, y + tileSize, color);    // Top-right
        
        // Triangle 2: bottom-left, top-right, top-left
        AddVertex(vertices, ref index, x, y, color);                          // Bottom-left
        AddVertex(vertices, ref index, x + tileSize, y + tileSize, color);    // Top-right
        AddVertex(vertices, ref index, x, y + tileSize, color);               // Top-left
    }
    
    private static void AddVertex(float[] vertices, ref int index, float x, float y, Vector3 color)
    {
        vertices[index++] = x;        // Position X
        vertices[index++] = y;        // Position Y
        vertices[index++] = color.X;  // Color R
        vertices[index++] = color.Y;  // Color G
        vertices[index++] = color.Z;  // Color B
    }
}