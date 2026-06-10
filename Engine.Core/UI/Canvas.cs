using OpenTK.Mathematics;
using Engine.Core.Rendering;

namespace Engine.Core.UI;

public class Canvas
{
    private SpriteBatch? _batch;
    private Camera _screenCamera = null!;
    private BitmapFont? _defaultFont;
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

    public void Begin(Camera? camera = null)
    {
        EnsureInitialized();
        if (camera == null)
            UpdateCamera();
        else
            _screenCamera = camera;

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

    public void DrawTextureRegion(Texture texture, float x, float y, float destWidth, float destHeight,
        float srcX, float srcY, float srcWidth, float srcHeight, Color4? color = null)
    {
        EnsureInitialized();
        var c = color ?? Color4.White;
        var sprite = new Sprite(texture, new Box2(srcX, srcY, srcX + srcWidth, srcY + srcHeight))
        {
            PixelsPerUnit = srcWidth / destWidth
        };
        var pos = new Vector2(x + destWidth * 0.5f, y + destHeight * 0.5f);
        _batch!.Draw(sprite, pos, Vector2.One, 0f, c, new Vector2(0.5f), 0);
    }
    public void DrawWorldTextCentered(BitmapFont font, string text, Vector2 worldPos, Color4 color, float scale = 1f)
    {
        EnsureInitialized();
        if (string.IsNullOrEmpty(text)) return;

        // 1. Вычисляем общую ширину текста для центрирования
        float totalWidth = 0f;
        foreach (var ch in text)
        {
            var glyph = font.GetGlyph(ch);
            if (glyph != null)
            {
                totalWidth += glyph.Value.AdvanceX * scale;
            }
        }

        var glyphH = font.GlyphHeight * scale;

        // Сдвигаем начальный x влево на половину ширины текста, чтобы отцентрировать по горизонтали
        float cursorX = worldPos.X - (totalWidth * 0.5f);
        // Сдвигаем начальный y вниз на половину высоты, чтобы отцентрировать по вертикали
        float cursorY = worldPos.Y + (glyphH * 0.5f);

        foreach (var ch in text)
        {
            if (ch == '\n') continue; // В мировом тексте для отладки переносы строк обычно не нужны

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

            // Рисуем спрайт буквы в мировых координатах
            var pos = new Vector2(cursorX + gw * 0.5f, cursorY - gh * 0.5f);
            _batch!.Draw(sprite, pos, new Vector2(scale), 0f, color, new Vector2(0.5f), 0);

            cursorX += g.AdvanceX * scale;
        }
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

    public void DrawWorldText(Vector2 worldPos, string text, Camera camera, Color4 color, float scale = 1f)
    {
        _defaultFont ??= BitmapFont.CreateDefault();
        var screenPos = camera.WorldToScreen(worldPos);
        DrawText(_defaultFont, text, screenPos.X, screenPos.Y, color, scale);
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
