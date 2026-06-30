using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace Engine.Core.Rendering;

public class Texture : IDisposable
{
    public int Handle { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] Data { get; private set; } = Array.Empty<byte>();

    public Texture(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Texture not found: {path}");

        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        Width = image.Width;
        Height = image.Height;
        Data = image.Data;

        CreateGlTexture(image.Data);
    }

    public Texture(int width, int height, byte[] data)
    {
        Width = width;
        Height = height;
        Data = data;
        CreateGlTexture(data);
    }

    private void CreateGlTexture(byte[] data)
    {
        Handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Handle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);
    }

    public void Bind()
    {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public static Texture CreateCheckerboard(int size = 32, uint color1 = 0xFF88FF88, uint color2 = 0xFF448844)
    {
        var data = new byte[size * size * 4];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var isColor1 = (x / (size / 8) + y / (size / 8)) % 2 == 0;
                var col = isColor1 ? color1 : color2;
                int idx = (y * size + x) * 4;
                data[idx + 0] = (byte)(col & 0xFF);
                data[idx + 1] = (byte)((col >> 8) & 0xFF);
                data[idx + 2] = (byte)((col >> 16) & 0xFF);
                data[idx + 3] = (byte)((col >> 24) & 0xFF);
            }
        }

        return new Texture(size, size, data);
    }

    public void Dispose()
    {
        if (Handle != 0)
            GL.DeleteTexture(Handle);
    }
}
