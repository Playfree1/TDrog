using OpenTK.Mathematics;

namespace Engine.Core.Rendering;

public class Sprite
{
    public Texture Texture { get; }
    public Vector2 Origin { get; set; }
    public Vector2 Size { get; set; }

    public Box2? SourceRect { get; set; }

    public float PixelsPerUnit { get; set; } = 100f;

    public Sprite(Texture texture)
    {
        Texture = texture;
        Size = new Vector2(texture.Width, texture.Height);
        SourceRect = new Box2(0, 0, texture.Width, texture.Height);
        Origin = new Vector2(0.5f);
    }

    public Sprite(Texture texture, Box2 sourceRect)
    {
        Texture = texture;
        Size = new Vector2(sourceRect.Size.X, sourceRect.Size.Y);
        SourceRect = sourceRect;
        Origin = new Vector2(0.5f);
    }
}
