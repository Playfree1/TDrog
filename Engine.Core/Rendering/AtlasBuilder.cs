using StbImageSharp;

namespace Engine.Core.Rendering;

public readonly record struct TileInfo(string Name, byte[] Pixels, int Width, int Height);

public class AtlasBuilder
{
    private readonly int _tileW, _tileH;
    private readonly List<TileInfo> _tiles = new();

    public AtlasBuilder(int tileWidth, int tileHeight)
    {
        _tileW = tileWidth;
        _tileH = tileHeight;
    }

    public int Add(string name, byte[] rgbaPixels, int width, int height)
    {
        if (width == _tileW && height == _tileH)
            _tiles.Add(new TileInfo(name, rgbaPixels, width, height));
        else
        {
            var resized = ResizePixels(rgbaPixels, width, height, _tileW, _tileH);
            _tiles.Add(new TileInfo(name, resized, _tileW, _tileH));
        }
        return _tiles.Count - 1;
    }

    private static byte[] ResizePixels(byte[] src, int srcW, int srcH, int dstW, int dstH)
    {
        var dst = new byte[dstW * dstH * 4];
        float scaleX = srcW / (float)dstW;
        float scaleY = srcH / (float)dstH;
        for (int dy = 0; dy < dstH; dy++)
        {
            float srcY = (dy + 0.5f) * scaleY - 0.5f;
            int sy = Math.Clamp((int)Math.Round(srcY), 0, srcH - 1);
            for (int dx = 0; dx < dstW; dx++)
            {
                float srcX = (dx + 0.5f) * scaleX - 0.5f;
                int sx = Math.Clamp((int)Math.Round(srcX), 0, srcW - 1);
                int si = (sy * srcW + sx) * 4;
                int di = (dy * dstW + dx) * 4;
                dst[di]     = src[si];
                dst[di + 1] = src[si + 1];
                dst[di + 2] = src[si + 2];
                dst[di + 3] = src[si + 3];
            }
        }
        return dst;
    }

    public int AddFromFile(string name, string path)
    {
        using var stream = File.OpenRead(path);
        var img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        return Add(name, img.Data, img.Width, img.Height);
    }

    public int Count => _tiles.Count;

    public AtlasBuildResult Build()
    {
        if (_tiles.Count == 0)
            throw new InvalidOperationException("No tiles added");

        int cols = (int)Math.Ceiling(Math.Sqrt(_tiles.Count));
        int rows = (int)Math.Ceiling((double)_tiles.Count / cols);
        int atlasW = cols * _tileW;
        int atlasH = rows * _tileH;

        var atlasData = new byte[atlasW * atlasH * 4];
        var indexMap = new Dictionary<string, int>(_tiles.Count);

        for (int i = 0; i < _tiles.Count; i++)
        {
            var tile = _tiles[i];
            int tileX = i % cols;
            int tileY = i / cols;

            for (int y = 0; y < _tileH; y++)
            {
                int srcOff = y * _tileW * 4;
                int dstOff = ((tileY * _tileH + y) * atlasW + tileX * _tileW) * 4;
                Array.Copy(tile.Pixels, srcOff, atlasData, dstOff, _tileW * 4);
            }

            indexMap[tile.Name] = i;
        }

        var atlas = new Texture(atlasW, atlasH, atlasData);
        return new AtlasBuildResult(atlas, indexMap);
    }
}

public class AtlasBuildResult
{
    public Texture Atlas { get; }
    private readonly Dictionary<string, int> _indices;

    public AtlasBuildResult(Texture atlas, Dictionary<string, int> indices)
    {
        Atlas = atlas;
        _indices = indices;
    }

    public int this[string name] =>
        _indices.TryGetValue(name, out var idx) ? idx : -1;

    public int IndexOf(string name) => this[name];

    public IReadOnlyCollection<string> Names => _indices.Keys;
}
