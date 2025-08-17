namespace Crawl.Renderer._2D.Tile;

public record TileDefinition(uint Id, string Name, Tile Tile)
{
   
   public override string ToString() => $"{Name} (ID: {Id})";
}

public class TileSet
{
   public string Name { get; }
   private readonly Dictionary<uint, TileDefinition> _tileDefs = new();

   public TileSet(string name)
   {
      Name = name;
      _tileDefs[0] = new TileDefinition(0,"empty",Tile.Empty);
   }

   public TileDefinition GetDefinition(uint id)
   {
      return _tileDefs.TryGetValue(id, out var tileDef) ? tileDef : _tileDefs[0];
   }

   public void Define(uint id, string name, Tile tile)
   {
      _tileDefs[id] = new TileDefinition(id, name, tile);
   }

   public bool HasDefinition(uint id) => _tileDefs.ContainsKey(id);
   public IEnumerable<TileDefinition> GetAllDefinitions() => _tileDefs.Values;
}

public class TileSetBuilder(string name)
{
   private readonly TileSet _tileSet = new(name);
   

   public TileSetBuilder Define(uint id, string name, Tile tile)
   {
      _tileSet.Define(id, name, tile);
      return this;
   }
    
   public TileSet Build() => _tileSet;
}