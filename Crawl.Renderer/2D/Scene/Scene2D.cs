using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Crawl.Renderer._2D.Rendering;
using Crawl.Renderer._2D.Texture;
using Crawl.Renderer._2D.Tile;

namespace Crawl.Renderer._2D.Scene;

/// <summary>
/// 2D scene containing a tile-based game world.
/// </summary>
public class Scene2D : AbstractScene, IDisposable
{
    private readonly TileMap _tileMap;
    private readonly TextureManager _textureManager;
    private readonly TileSet _tileSet;
    private readonly TileRenderer _renderer;
    private readonly float[] _cachedVertices;
    private bool _disposed;

    private readonly uint[,] _tileLayout =
    {
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    };


    public Scene2D()
    {
        // Initialize texture manager
        _textureManager = CreateTextureManager();
        
        // Initialize tileset
        _tileSet = CreateTileSet();
            
        // Initialize tilemap
        _tileMap = new TileMap(_tileLayout, _tileSet);
        
        // Initialize renderer
        _renderer = new TileRenderer();
        
        // Generate and cache vertex data
        _cachedVertices = TileVertexGenerator.GenerateVertices(_tileLayout, _textureManager);
        
        // Debug output
        Console.WriteLine($"Scene2D initialized: {_tileMap.TileCount} tiles, {_cachedVertices.Length} vertex values");
    }
    
    private static TextureManager CreateTextureManager()
    {
        var manager = new TextureManager();
        manager.Add(0, new TextureData(Colors.Void));
        manager.Add(1, new TextureData(Colors.StoneWall));
        manager.Add(2, new TextureData(Colors.ShallowWater));
        return manager;
    }
    
    private static TileSet CreateTileSet()
    {
        var builder = new TileSetBuilder("test_tiles");
        return builder
            .Define(0, "void", new Tile.Tile(0, 1f))
            .Define(1, "stone", new Tile.Tile(1, 0))
            .Define(2, "water", new Tile.Tile(2, 0.5f))
            .Build();
    }

    /// <summary>
    /// Handles mouse click events for tile interaction.
    /// </summary>
    public void OnMouseUp(MouseState mouseState)
    {
        var tilePos = CoordinateConverter.ScreenToTile(
            mouseState.X, mouseState.Y, 
            RenderConfig.DefaultWindowWidth, RenderConfig.DefaultWindowHeight);
        
        var worldPos = CoordinateConverter.ScreenToWorld(
            mouseState.X, mouseState.Y, 
            RenderConfig.DefaultWindowWidth, RenderConfig.DefaultWindowHeight);
        
        // Check if click is within bounds
        if (CoordinateConverter.IsValidTilePosition(tilePos.X, tilePos.Y, _tileLayout.GetLength(1), _tileLayout.GetLength(0)))
        {
            var tileId = _tileLayout[tilePos.Y, tilePos.X];
            var tile = _tileMap.GetTile(tilePos.Y, tilePos.X);
            var tileDef = _tileSet.GetDefinition(tile.TileId);
            
            Console.WriteLine($"Clicked tile at ({tilePos.X}, {tilePos.Y}) - TileId: {tileId}, Tile: {tile} Type: {tileDef.Name}");
        }
        else
        {
            Console.WriteLine($"Clicked outside map bounds at world ({worldPos.X:F1}, {worldPos.Y:F1}) -> tile ({tilePos.X}, {tilePos.Y})");
        }
    }
    
    /// <summary>
    /// Renders the scene to the current OpenGL context.
    /// </summary>
    public override void Draw()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Scene2D));
            
        var totalTiles = _tileLayout.GetLength(0) * _tileLayout.GetLength(1);
        _renderer.Render(_cachedVertices, totalTiles);
    }
    
    /// <summary>
    /// Disposes of OpenGL resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        
        _renderer?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    
    ~Scene2D()
    {
        if (!_disposed)
        {
            Console.WriteLine("Warning: Scene2D was not properly disposed!");
        }
    }
    
}