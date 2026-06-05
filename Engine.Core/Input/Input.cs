using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Core.Input;

public static class Input
{
    internal static void Update()
    {
    }

    public static bool GetKey(Keys key) => Keyboard.IsKeyDown(key);
    public static bool GetKeyDown(Keys key) => Keyboard.IsKeyPressed(key);

    public static bool GetMouseButton(int button) => Mouse.IsButtonDown((MouseButton)button);
    public static bool GetMouseButtonDown(int button) => Mouse.IsButtonPressed((MouseButton)button);
    public static bool GetMouseButtonUp(int button) => Mouse.IsButtonReleased((MouseButton)button);

    public static Vector2 MousePosition => Mouse.Position;
    public static Vector2 MouseDelta => Mouse.Delta;
    public static float MouseScrollDelta => Mouse.ScrollDelta;
}
