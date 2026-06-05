using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Core.Rendering;

public static class Renderer
{
    public static int ScreenWidth { get; set; } = 800;
    public static int ScreenHeight { get; set; } = 600;
    internal static Matrix4 ViewProjection;
    public static Color4 ClearColor { get; set; } = new Color4(0.2f, 0.25f, 0.3f, 1f);
    public static Camera? MainCamera { get; set; }
    internal static SpriteBatch? Batch { get; set; }

    public static int DebugSpriteCount => Batch?.DebugSpriteCount ?? 0;

    private static Shader? _chunkShader;

    public static void Initialize(int maxSprites = 2048)
    {
        Batch = new SpriteBatch(maxSprites);
    }

    public static void Begin(Camera camera)
    {
        MainCamera = camera;
        ViewProjection = camera.ViewProjectionMatrix;
        Batch?.Begin(camera);
    }

    public static void Draw(Sprite sprite, Vector2 position, Vector2 scale, float rotation,
        Color4 color, Vector2 origin, float depth)
    {
        Batch?.Draw(sprite, position, scale, rotation, color, origin, depth);
    }

    public static void End()
    {
        Batch?.End();
    }

    internal static void DrawChunk(Texture texture, int indexCount)
    {
        if (_chunkShader == null)
        {
            _chunkShader = Batch?._shader;
            if (_chunkShader == null) return;
        }

        _chunkShader.Use();
        _chunkShader.SetMatrix4("uViewProjection", ViewProjection);
        texture.Bind();
        GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
    }
}
