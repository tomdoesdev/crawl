using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Crawl.Renderer._2D.Rendering;

/// <summary>
/// Handles OpenGL rendering of tile-based 2D scenes.
/// </summary>
public class TileRenderer : IDisposable
{
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _shaderProgram;
    private readonly int _projectionLocation;
    private bool _disposed;

    /// <summary>
    /// Creates a new TileRenderer with compiled shaders and buffer setup.
    /// </summary>
    public TileRenderer()
    {
        // Generate OpenGL objects
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        
        // Setup VAO
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        
        // Configure vertex attributes
        SetupVertexAttributes();
        
        // Create and compile shaders
        _shaderProgram = CreateShaderProgram();
        _projectionLocation = GL.GetUniformLocation(_shaderProgram, "projection");
        
        // Cleanup bindings
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        
        CheckGLError("TileRenderer initialization");
    }

    /// <summary>
    /// Renders a tilemap using the provided vertex data.
    /// </summary>
    public void Render(float[] vertices, int tileCount)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TileRenderer));
            
        // Upload vertex data
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        
        // Use shader and setup rendering state
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(_vao);
        
        // Draw tiles
        GL.DrawArrays(PrimitiveType.Triangles, 0, tileCount * RenderConfig.VerticesPerTile);
        
        // Cleanup bindings
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        CheckGLError("TileRenderer.Render");
    }

    private void SetupVertexAttributes()
    {
        var stride = RenderConfig.VertexSizeBytes;
        
        // Position attribute (location 0) - 2 floats
        GL.VertexAttribPointer(0, RenderConfig.PositionComponents, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        
        // Color attribute (location 1) - 3 floats
        var colorOffset = RenderConfig.PositionComponents * sizeof(float);
        GL.VertexAttribPointer(1, RenderConfig.ColorComponents, VertexAttribPointerType.Float, false, stride, colorOffset);
        GL.EnableVertexAttribArray(1);
    }

    private static int CreateShaderProgram()
    {
        // Vertex shader source
        const string vertexShaderSource = @"
#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec3 aColor;

out vec3 vertexColor;

void main()
{
    // Scale and center the tiles, flip Y coordinate
    vec2 normalized;
    normalized.x = aPosition.x / 200.0 - 1.0;     // X: scale and center
    normalized.y = 1.0 - aPosition.y / 150.0;     // Y: flip and scale  
    gl_Position = vec4(normalized, 0.0, 1.0);
    vertexColor = aColor;
}";

        // Fragment shader source
        const string fragmentShaderSource = @"
#version 330 core
in vec3 vertexColor;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vertexColor, 1.0);
}";

        // Compile vertex shader
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckShaderCompilation(vertexShader, "vertex");

        // Compile fragment shader
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckShaderCompilation(fragmentShader, "fragment");

        // Link program
        var program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);
        CheckProgramLinking(program);

        // Cleanup shaders
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        return program;
    }

    private static void CheckShaderCompilation(int shader, string shaderType)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
        if (status == 0)
        {
            var log = GL.GetShaderInfoLog(shader);
            throw new InvalidOperationException($"Failed to compile {shaderType} shader: {log}");
        }
    }

    private static void CheckProgramLinking(int program)
    {
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
        if (status == 0)
        {
            var log = GL.GetProgramInfoLog(program);
            throw new InvalidOperationException($"Failed to link shader program: {log}");
        }
    }

    private static void CheckGLError(string operation)
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            throw new InvalidOperationException($"OpenGL error during {operation}: {error}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        GL.DeleteVertexArray(_vao);
        GL.DeleteBuffer(_vbo);
        GL.DeleteProgram(_shaderProgram);

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~TileRenderer()
    {
        if (!_disposed)
        {
            Console.WriteLine("Warning: TileRenderer was not properly disposed!");
        }
    }
}