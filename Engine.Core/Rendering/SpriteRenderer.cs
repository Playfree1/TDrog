using Engine.Core.GameObjects;
using Engine.Core.Animation;
using OpenTK.Mathematics;

namespace Engine.Core.Rendering;

public class SpriteRenderer : Component
{
    public Sprite? Sprite { get; set; }
    public Color4 Color { get; set; } = Color4.White;
    public Vector2 Origin { get; set; } = new(0.5f);
    public float SortingOrder { get; set; }
    public Vector2 ScaleOverride { get; set; } = Vector2.One;
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }

    private Animator? _animator;

    public override void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public override void Render()
    {
        if (Sprite == null) return;

        var animSprite = _animator?.GetCurrentSprite();
        var renderSprite = animSprite ?? Sprite;

        var pos = Transform.WorldPosition;
        var scale = Transform.Scale * ScaleOverride;
        var rot = Transform.Rotation;

        if (Renderer.MainCamera != null)
        {
            var worldSize = renderSprite.Size / renderSprite.PixelsPerUnit * scale;
            if (!Renderer.MainCamera.IsVisible(pos, worldSize * 0.5f))
                return;
        }

        var origin = Origin;
        if (FlipX) origin.X = 1f - origin.X;
        if (FlipY) origin.Y = 1f - origin.Y;

        Renderer.Draw(renderSprite, pos, scale, rot, Color, origin, SortingOrder);
    }
}
