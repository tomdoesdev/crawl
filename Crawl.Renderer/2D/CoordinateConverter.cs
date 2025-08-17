using OpenTK.Mathematics;

namespace Crawl.Renderer._2D;

/// <summary>
/// Utility class for converting between different coordinate systems in the 2D renderer.
/// </summary>
public static class CoordinateConverter
{
    /// <summary>
    /// Converts screen coordinates to world coordinates using the current shader transformation.
    /// </summary>
    /// <param name="screenX">Screen X coordinate (0 to windowWidth)</param>
    /// <param name="screenY">Screen Y coordinate (0 to windowHeight)</param>
    /// <param name="windowWidth">Window width in pixels</param>
    /// <param name="windowHeight">Window height in pixels</param>
    /// <returns>World coordinates as Vector2</returns>
    public static Vector2 ScreenToWorld(float screenX, float screenY, int windowWidth, int windowHeight)
    {
        // Convert screen coordinates to NDC (-1 to 1)
        var ndcX = (2.0f * screenX) / windowWidth - 1.0f;
        var ndcY = 1.0f - (2.0f * screenY) / windowHeight;
        
        // Reverse the shader transformation to get world coordinates
        // Shader does: normalized.x = aPosition.x / WorldToNdcScaleX - 1.0
        // So: aPosition.x = (normalized.x + 1.0) * WorldToNdcScaleX
        var worldX = (ndcX + 1.0f) * RenderConfig.WorldToNdcScaleX;
        
        // Shader does: normalized.y = 1.0 - aPosition.y / WorldToNdcScaleY  
        // So: aPosition.y = (1.0 - normalized.y) * WorldToNdcScaleY
        var worldY = (1.0f - ndcY) * RenderConfig.WorldToNdcScaleY;
        
        return new Vector2(worldX, worldY);
    }
    
    /// <summary>
    /// Converts world coordinates to screen coordinates using the current shader transformation.
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <param name="windowWidth">Window width in pixels</param>
    /// <param name="windowHeight">Window height in pixels</param>
    /// <returns>Screen coordinates as Vector2</returns>
    public static Vector2 WorldToScreen(float worldX, float worldY, int windowWidth, int windowHeight)
    {
        // Apply shader transformation to get NDC
        var ndcX = worldX / RenderConfig.WorldToNdcScaleX - 1.0f;
        var ndcY = 1.0f - worldY / RenderConfig.WorldToNdcScaleY;
        
        // Convert NDC to screen coordinates
        var screenX = (ndcX + 1.0f) * windowWidth / 2.0f;
        var screenY = (1.0f - ndcY) * windowHeight / 2.0f;
        
        return new Vector2(screenX, screenY);
    }
    
    /// <summary>
    /// Converts world coordinates to tile coordinates.
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldY">World Y coordinate</param>
    /// <param name="tileSize">Size of each tile in world units</param>
    /// <returns>Tile coordinates as Vector2i</returns>
    public static Vector2i WorldToTile(float worldX, float worldY, float tileSize = RenderConfig.TileSize)
    {
        return new Vector2i((int)(worldX / tileSize), (int)(worldY / tileSize));
    }
    
    /// <summary>
    /// Converts tile coordinates to world coordinates (center of tile).
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <param name="tileSize">Size of each tile in world units</param>
    /// <returns>World coordinates at the center of the tile</returns>
    public static Vector2 TileToWorld(int tileX, int tileY, float tileSize = RenderConfig.TileSize)
    {
        return new Vector2(tileX * tileSize + tileSize / 2.0f, tileY * tileSize + tileSize / 2.0f);
    }
    
    /// <summary>
    /// Converts screen coordinates directly to tile coordinates.
    /// </summary>
    /// <param name="screenX">Screen X coordinate</param>
    /// <param name="screenY">Screen Y coordinate</param>
    /// <param name="windowWidth">Window width in pixels</param>
    /// <param name="windowHeight">Window height in pixels</param>
    /// <param name="tileSize">Size of each tile in world units</param>
    /// <returns>Tile coordinates as Vector2i</returns>
    public static Vector2i ScreenToTile(float screenX, float screenY, int windowWidth, int windowHeight, float tileSize = RenderConfig.TileSize)
    {
        var worldPos = ScreenToWorld(screenX, screenY, windowWidth, windowHeight);
        return WorldToTile(worldPos.X, worldPos.Y, tileSize);
    }
    
    /// <summary>
    /// Checks if the given tile coordinates are within the bounds of a tilemap.
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <param name="mapWidth">Width of the tilemap in tiles</param>
    /// <param name="mapHeight">Height of the tilemap in tiles</param>
    /// <returns>True if coordinates are within bounds</returns>
    public static bool IsValidTilePosition(int tileX, int tileY, int mapWidth, int mapHeight)
    {
        return tileX >= 0 && tileX < mapWidth && tileY >= 0 && tileY < mapHeight;
    }
}