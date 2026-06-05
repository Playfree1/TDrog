using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Core;

public class Game : GameWindow
{
    private Scene.Scene? _currentScene;
    private Scene.Scene? _nextScene;

    public static long LastUpdateUs { get; private set; }
    public static long LastRenderUs { get; private set; }

    public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        Rendering.Renderer.ScreenWidth = ClientSize.X;
        Rendering.Renderer.ScreenHeight = ClientSize.Y;
        Rendering.Renderer.Initialize();

        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        Input.Keyboard.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        Input.Keyboard.OnKeyUp(e);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Input.Mouse.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        Input.Mouse.OnMouseUp(e);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        var mouseState = MouseState;
        Input.Mouse.Update(mouseState);

        var dt = (float)args.Time;
        Time.UnscaledDeltaTime = dt;
        Time.DeltaTime = dt * Time.TimeScale;
        Time.ElapsedTime += Time.UnscaledDeltaTime;
        Time.FrameCount++;

        if (_nextScene != null)
        {
            _currentScene?.Unload();
            _currentScene = _nextScene;
            _currentScene.Load();
            _nextScene = null;
        }

        if (_currentScene?.IsLoaded == true)
        {
            if (IsFocused)
            {
                Time.FixedDeltaTimeAccumulator += Time.DeltaTime;
                while (Time.FixedDeltaTimeAccumulator >= Time.FixedDeltaTime)
                {
                    _currentScene.FixedUpdate(Time.FixedDeltaTime);
                    Time.FixedDeltaTimeAccumulator -= Time.FixedDeltaTime;
                }

                var swUpdate = Stopwatch.StartNew();
                _currentScene.Update(Time.DeltaTime);
                swUpdate.Stop();
                LastUpdateUs = swUpdate.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;
            }
        }

    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        var swRender = Stopwatch.StartNew();

        GL.ClearColor(Rendering.Renderer.ClearColor);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        if (_currentScene?.IsLoaded == true)
        {
            if (Rendering.Renderer.MainCamera != null)
            {
                Rendering.Renderer.Begin(Rendering.Renderer.MainCamera);
                _currentScene.Render();
                Rendering.Renderer.End();
            }
        }

        swRender.Stop();
        LastRenderUs = swRender.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        Rendering.Renderer.ScreenWidth = ClientSize.X;
        Rendering.Renderer.ScreenHeight = ClientSize.Y;
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
    }

    protected override void OnUnload()
    {
        _currentScene?.Unload();
        base.OnUnload();
    }

    public void LoadScene(Scene.Scene scene)
    {
        _nextScene = scene;
    }

    public void Run(Scene.Scene startupScene)
    {
        _nextScene = startupScene;
        Run();
    }
}
