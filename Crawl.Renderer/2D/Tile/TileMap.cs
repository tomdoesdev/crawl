
namespace Crawl.Renderer._2D.Tile;

public class TileMap
{
    private readonly Dictionary<long, Tile> _tiles = new();
    private readonly uint _cellSize;
    
    // Cached bounds for performance
    private int _minX = int.MaxValue;
    private int _minY = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _maxY = int.MinValue;
    private bool _boundsValid = false;

    public int TileCount => _tiles.Count;

    public TileMap(uint[,] layout, TileSet tileSet, uint cellSize = 1)
    {
        _cellSize = cellSize;

        var rows = layout.GetLength(0);
        var cols = layout.GetLength(1);

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                var tileDef = tileSet.GetDefinition(layout[row, col]);

                if (tileDef.Tile == Tile.Empty)
                {
                    continue;
                }
                
                SetTile(row, col, tileDef.Tile);
            }
        }
    }


    public void SetTile(int x, int y, Tile tile)
    {
        if (tile.TileId == 0)
        {
            //Tiles with ID = 0 are 'empty', so remove them for now.
            RemoveTile(x, y);
            return;
        }
        
        _tiles.Add(GetHashKey(x,y),tile);
        UpdateBounds(x, y);
    }

    public Tile GetTile(int x, int y)
    {
        return _tiles.TryGetValue(GetHashKey(x, y), out var tile) ? tile : Tile.Empty;
    }

    public bool HasTile(int x, int y)
    {
        return _tiles.ContainsKey(GetHashKey(x, y));
    }

    public bool RemoveTile(int x, int y)
    {
        var removed = _tiles.Remove(GetHashKey(x, y));
        if (removed)
        {
            _boundsValid = false; // Invalidate bounds cache
        }
        return removed;
    }
    
    public IEnumerable<(int x, int y, Tile tile)> GetTilesInRegion(int minX, int minY, int maxX, int maxY)
    {
        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var tile = GetTile(x, y);
                if (tile.TileId != 0)
                {
                    yield return (x, y, tile);
                }
            }
        }
    }
    
    public IEnumerable<(int x, int y, Tile tile)> GetAllTiles()
    {
        /*
         * foreach (var kvp in _tiles)
           {
               var coords = UnhashKey(kvp.Key);
               yield return (coords.x, coords.y, kvp.Value);
           }
         */
        return from kvp in _tiles let coords = UnhashKey(kvp.Key) select (coords.x, coords.y, kvp.Value);
    }

    public void Clear()
    {
        _tiles.Clear();
        _boundsValid = false; // Invalidate bounds cache
    }
    
    /// <summary>
    /// Gets the bounds of all non-empty tiles in the tilemap.
    /// Returns null if no tiles exist.
    /// </summary>
    public (int minX, int minY, int maxX, int maxY)? GetBounds()
    {
        if (_tiles.Count == 0)
            return null;
            
        if (!_boundsValid)
        {
            RecalculateBounds();
        }
        
        return (_minX, _minY, _maxX, _maxY);
    }
    
    /// <summary>
    /// Checks if the given coordinate is within the bounds of the tilemap.
    /// </summary>
    public bool IsWithinBounds(int x, int y)
    {
        var bounds = GetBounds();
        if (!bounds.HasValue)
            return false;
            
        var (minX, minY, maxX, maxY) = bounds.Value;
        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }
    
    /// <summary>
    /// Gets the width and height of the tilemap in tiles.
    /// </summary>
    public (int width, int height) GetDimensions()
    {
        var bounds = GetBounds();
        if (!bounds.HasValue)
            return (0, 0);
            
        var (minX, minY, maxX, maxY) = bounds.Value;
        return (maxX - minX + 1, maxY - minY + 1);
    }
    
    private void UpdateBounds(int x, int y)
    {
        _minX = Math.Min(_minX, x);
        _minY = Math.Min(_minY, y);
        _maxX = Math.Max(_maxX, x);
        _maxY = Math.Max(_maxY, y);
        _boundsValid = true;
    }
    
    private void RecalculateBounds()
    {
        if (_tiles.Count == 0)
        {
            _boundsValid = false;
            return;
        }
        
        _minX = int.MaxValue;
        _minY = int.MaxValue;
        _maxX = int.MinValue;
        _maxY = int.MinValue;
        
        foreach (var kvp in _tiles)
        {
            var (x, y) = UnhashKey(kvp.Key);
            UpdateBounds(x, y);
        }
    }
    
    private static long GetHashKey(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }
    
    private static (int x, int y) UnhashKey(long key)
    {
        var x = (int)(key >> 32);
        var y = (int)(key & 0xFFFFFFFF);
        return (x, y);
    }
    
    
}