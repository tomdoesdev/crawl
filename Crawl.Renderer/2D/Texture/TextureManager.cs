using OpenTK.Mathematics;

namespace Crawl.Renderer._2D.Texture;

public class TextureManager
{
    private readonly Dictionary<uint, TextureData> _textures = new();

    /// <summary>
    /// Adds a new texture. Throws if texture ID already exists.
    /// </summary>
    public void Add(uint textureId, TextureData data)
    {
        if (_textures.ContainsKey(textureId))
            throw new ArgumentException($"Texture with ID {textureId} already exists");
            
        _textures.Add(textureId, data);
    }

    /// <summary>
    /// Gets a texture by ID. Returns default TextureData if not found.
    /// </summary>
    public TextureData Get(uint textureId)
    {
        return _textures.GetValueOrDefault(textureId, new TextureData(Vector3.Zero));
    }
    
    /// <summary>
    /// Tries to get a texture by ID.
    /// </summary>
    public bool TryGet(uint textureId, out TextureData data)
    {
        return _textures.TryGetValue(textureId, out data);
    }
    
    /// <summary>
    /// Gets a required texture. Throws if not found.
    /// </summary>
    public TextureData GetRequired(uint textureId)
    {
        return _textures.TryGetValue(textureId, out var data) 
            ? data 
            : throw new ArgumentException($"Required texture {textureId} not found");
    }
    
    /// <summary>
    /// Checks if a texture exists.
    /// </summary>
    public bool Contains(uint textureId) => _textures.ContainsKey(textureId);
    
    /// <summary>
    /// Gets all registered texture IDs.
    /// </summary>
    public IEnumerable<uint> GetAllTextureIds() => _textures.Keys;
    
    /// <summary>
    /// Gets the number of registered textures.
    /// </summary>
    public int Count => _textures.Count;
}

/// <summary>
/// Immutable texture data containing color information.
/// </summary>
public readonly record struct TextureData(Vector3 Color);

