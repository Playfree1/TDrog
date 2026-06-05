using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Core.Input;

public static class Mouse
{
    private static MouseState? _currentState;
    private static int _leftClickPending;
    private static int _rightClickPending;
    private static int _middleClickPending;
    private static readonly HashSet<MouseButton> _downButtons = new();
    private static int _leftReleasePending;
    private static int _rightReleasePending;

    private static Vector2 _position;
    private static Vector2 _delta;
    private static float _scrollDelta;

    internal static void Update(MouseState state)
    {
        _currentState = state;
        _position = new Vector2(state.X, state.Y);
        _delta = new Vector2(state.Delta.X, state.Delta.Y);
        _scrollDelta = state.ScrollDelta.Y;
    }

    internal static void OnMouseDown(MouseButtonEventArgs e)
    {
        _downButtons.Add(e.Button);
        switch (e.Button)
        {
            case MouseButton.Left: _leftClickPending++; break;
            case MouseButton.Right: _rightClickPending++; break;
            case MouseButton.Middle: _middleClickPending++; break;
        }
    }

    internal static void OnMouseUp(MouseButtonEventArgs e)
    {
        _downButtons.Remove(e.Button);
        switch (e.Button)
        {
            case MouseButton.Left: _leftReleasePending++; break;
            case MouseButton.Right: _rightReleasePending++; break;
        }
    }

    public static Vector2 Position => _position;
    public static Vector2 Delta => _delta;
    public static float ScrollDelta => _scrollDelta;

    public static bool IsButtonDown(MouseButton button) => _downButtons.Contains(button);

    public static bool IsButtonReleased(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                if (_leftReleasePending > 0) { _leftReleasePending--; return true; }
                return false;
            case MouseButton.Right:
                if (_rightReleasePending > 0) { _rightReleasePending--; return true; }
                return false;
            default: return false;
        }
    }

    public static bool IsButtonPressed(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                if (_leftClickPending > 0) { _leftClickPending--; return true; }
                return false;
            case MouseButton.Right:
                if (_rightClickPending > 0) { _rightClickPending--; return true; }
                return false;
            case MouseButton.Middle:
                if (_middleClickPending > 0) { _middleClickPending--; return true; }
                return false;
            default: return false;
        }
    }
}
