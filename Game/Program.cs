using System;
using System.Runtime.InteropServices; // Добавлено для MessageBox
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using TowerDefecse;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;

try
{
    var nativeSettings = new NativeWindowSettings
    {
        ClientSize = new Vector2i(800, 600),
        Title = "Test Game",
        Flags = ContextFlags.ForwardCompatible,
        Profile = ContextProfile.Core,
        API = ContextAPI.OpenGL,
        APIVersion = new Version(3, 3),
        AutoLoadBindings = false
    };

    using var game = new StartGame(GameWindowSettings.Default, nativeSettings);

    // Если упадет тут (например, нет поддержки OpenGL 3.3) — мы это поймаем
    GL.LoadBindings(new GLFWBindingsContext());

    // Тут крутится игра. Любая ошибка внутри классов TowerDefecse прилетит сюда
    game.Run();
}
catch (Exception ex)
{
    // Показываем нативное окно ошибки Windows
    NativeMethods.ShowError($"Ты всё сломал! :( В любом случае срочно отправь всю ниже перечисленную ерунду разработчику пусть он страдает(ну или можешь разобраться сам)\n\nОшибка:\n{ex.Message}\n\nСтек вызова:\n{ex.StackTrace}");

    // Принудительно закрываем процесс с кодом ошибки
    Environment.Exit(1);
}

// Выносим вызов Win32 API в конец файла
internal static class NativeMethods
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static void ShowError(string message)
    {
        // 0x10 — красная иконка "Ошибка" (X), 0x00 — кнопка "ОК"
        MessageBox(IntPtr.Zero, message, "Game - Crash Report", 0x10 | 0x00);
    }
}
