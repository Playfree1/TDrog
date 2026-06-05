using OpenTK.Mathematics;
using Engine.Core.Rendering;

namespace Engine.Core.UI;

public class Canvas
{
    private SpriteBatch? _batch;
    private Camera _screenCamera = null!;
    private bool _initialized;

    public Canvas()
    {
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;
        _batch = new SpriteBatch();
        UpdateCamera();
        _initialized = true;
    }

    public void Begin()
    {
        EnsureInitialized();
        UpdateCamera();
        _batch!.Begin(_screenCamera);
    }

    public void DrawTexture(Texture texture, float x, float y, float width, float height, Color4? color = null)
    {
        EnsureInitialized();
        var c = color ?? Color4.White;
        var sprite = new Sprite(texture)
        {
            PixelsPerUnit = texture.Width / width
        };
        var pos = new Vector2(x + width * 0.5f, y + height * 0.5f);
        _batch!.Draw(sprite, pos, Vector2.One, 0f, c, new Vector2(0.5f), 0);
    }

    public void DrawText(BitmapFont font, string text, float x, float y, Color4 color, float scale = 1f)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(text)) return;

        var glyphH = font.GlyphHeight * scale;
        float cursorX = x;
        float cursorY = y + glyphH;

        foreach (var ch in text)
        {
            if (ch == '\n')
            {
                cursorX = x;
                cursorY += glyphH + font.LineSpacing;
                continue;
            }

            var glyph = font.GetGlyph(ch);
            if (glyph == null) continue;

            var g = glyph.Value;
            var gw = g.Width * scale;
            var gh = g.Height * scale;

            var sprite = new Sprite(font.Texture)
            {
                SourceRect = new Box2(g.X, g.Y, g.X + g.Width, g.Y + g.Height),
                PixelsPerUnit = g.Width / gw
            };

            var pos = new Vector2(cursorX + gw * 0.5f, cursorY - gh * 0.5f);
            _batch!.Draw(sprite, pos, new Vector2(scale), 0f, color, new Vector2(0.5f), 0);

            cursorX += g.AdvanceX * scale;
        }
    }

    public void End()
    {
        if (_initialized)
            _batch!.End();
    }

    private void UpdateCamera()
    {
        _screenCamera = new Camera
        {
            Position = new Vector2(Renderer.ScreenWidth * 0.5f, Renderer.ScreenHeight * 0.5f),
            Width = Renderer.ScreenHeight,
            Zoom = 1f
        };
    }
}
