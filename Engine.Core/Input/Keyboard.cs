using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Core.Input;

public static class Keyboard
{
    private static readonly HashSet<Keys> _keysDown = new();
    private static readonly Dictionary<Keys, int> _keyPressCounts = new();

    internal static void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (!_keysDown.Contains(e.Key))
        {
            _keysDown.Add(e.Key);
            _keyPressCounts.TryGetValue(e.Key, out int count);
            _keyPressCounts[e.Key] = count + 1;
        }
    }

    internal static void OnKeyUp(KeyboardKeyEventArgs e)
    {
        _keysDown.Remove(e.Key);
    }

    public static bool IsKeyDown(Keys key) => _keysDown.Contains(key);

    public static bool IsKeyPressed(Keys key)
    {
        if (_keyPressCounts.TryGetValue(key, out int count) && count > 0)
        {
            _keyPressCounts[key] = count - 1;
            return true;
        }
        return false;
    }
}
