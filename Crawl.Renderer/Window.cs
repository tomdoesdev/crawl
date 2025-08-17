using System.Drawing;
using Crawl.Renderer._2D.Scene;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Crawl.Renderer;

public class Window : GameWindow
{
    private readonly Scene2D _scene;

    public Window(string title, int width, int height): base(GameWindowSettings.Default, new NativeWindowSettings())
    {
        ClientSize = new Vector2i(width, height);
        Title = title;

        _scene = new Scene2D();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.ClearColor(Color.Navy);
        _scene.Draw();
        
        SwapBuffers();
        base.OnRenderFrame(args);
    }
};

