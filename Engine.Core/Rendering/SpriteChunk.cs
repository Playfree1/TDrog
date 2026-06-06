using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Engine.Core.GameObjects;

namespace Engine.Core.Rendering;

public class SpriteChunk : Component, IDisposable
{
    public Sprite? Sprite { get; set; }
    public Vector2 DefaultScale { get; set; } = Vector2.One;
    public Vector2 DefaultOrigin { get; set; } = new(0.5f);
    public int SortingOrder { get; set; }

    private struct ChunkEntry
    {
        public Vector2 Position;
        public Vector2 Scale;
        public float Rotation;
        public Color4 Color;
    }

    private ChunkEntry[] _entries = Array.Empty<ChunkEntry>();
    private int _count;
    private bool _dirty = true;

    private const int VertexStride = 8;

    private float[] _vertices = Array.Empty<float>();
    private int _vertexCount;

    private int _vbo = -1;
    private int _vao = -1;
    private int _ebo = -1;
    private int _uploadedVertices;

    public void Clear()
    {
        _count = 0;
        _dirty = true;
    }

    public void Add(Vector2 position, Vector2? scale = null, Color4? color = null, float rotation = 0f)
    {
        if (_entries.Length <= _count)
            Array.Resize(ref _entries, Math.Max(64, _entries.Length * 2));

        _entries[_count++] = new ChunkEntry
        {
            Position = position,
            Scale = scale ?? DefaultScale,
            Rotation = rotation,
            Color = color ?? Color4.White
        };
        _dirty = true;
    }

    public void AddRange(Vector2[] positions, Vector2? scale = null, Color4? color = null, float rotation = 0f)
    {
        var s = scale ?? DefaultScale;
        var c = color ?? Color4.White;
        var needed = _count + positions.Length;

        if (_entries.Length < needed)
            Array.Resize(ref _entries, needed);

        for (int i = 0; i < positions.Length; i++)
        {
            _entries[_count++] = new ChunkEntry
            {
                Position = positions[i],
                Scale = s,
                Rotation = rotation,
                Color = c
            };
        }
        _dirty = true;
    }

    public int Count => _count;

    public override void Render()
    {
        if (Sprite == null || _count == 0) return;

        if (_dirty)
        {
            BuildVertices();
            UploadToGPU();
        }

        if (_uploadedVertices == 0) return;

        var indexCount = _uploadedVertices / 4 * 6;

        GL.BindVertexArray(_vao);
        Renderer.DrawChunk(Sprite!.Texture, indexCount);
        GL.BindVertexArray(0);
    }

    private void BuildVertices()
    {
        var w = Sprite!.Size.X / Sprite.PixelsPerUnit;
        var h = Sprite.Size.Y / Sprite.PixelsPerUnit;
        var ox = DefaultOrigin.X * w;
        var oy = DefaultOrigin.Y * h;

        var src = Sprite.SourceRect!.Value;
        var texW = Sprite.Texture.Width;
        var texH = Sprite.Texture.Height;

        float uMin = src.Min.X / texW;
        float uMax = src.Max.X / texW;
        float vMin = src.Min.Y / texH;
        float vMax = src.Max.Y / texH;

        if (_vertices.Length < _count * 4 * VertexStride)
            _vertices = new float[_count * 4 * VertexStride];

        _vertexCount = 0;

        for (int i = 0; i < _count; i++)
        {
            var e = _entries[i];
            var px = e.Position.X;
            var py = e.Position.Y;
            var sx = e.Scale.X;
            var sy = e.Scale.Y;

            float cosR = (float)Math.Cos(e.Rotation);
            float sinR = (float)Math.Sin(e.Rotation);

            float lx0 = (-ox) * sx;
            float ly0 = (-oy) * sy;
            float lx1 = (w - ox) * sx;
            float ly1 = (-oy) * sy;
            float lx2 = (w - ox) * sx;
            float ly2 = (h - oy) * sy;
            float lx3 = (-ox) * sx;
            float ly3 = (h - oy) * sy;

            float x0 = lx0 * cosR - ly0 * sinR + px;
            float y0 = lx0 * sinR + ly0 * cosR + py;
            float x1 = lx1 * cosR - ly1 * sinR + px;
            float y1 = lx1 * sinR + ly1 * cosR + py;
            float x2 = lx2 * cosR - ly2 * sinR + px;
            float y2 = lx2 * sinR + ly2 * cosR + py;
            float x3 = lx3 * cosR - ly3 * sinR + px;
            float y3 = lx3 * sinR + ly3 * cosR + py;

            int idx = _vertexCount * VertexStride;

            _vertices[idx + 0]  = x0; _vertices[idx + 1]  = y0;
            _vertices[idx + 2]  = uMin; _vertices[idx + 3]  = vMax;
            _vertices[idx + 4]  = e.Color.R; _vertices[idx + 5]  = e.Color.G;
            _vertices[idx + 6]  = e.Color.B; _vertices[idx + 7]  = e.Color.A;

            _vertices[idx + 8]  = x1; _vertices[idx + 9]  = y1;
            _vertices[idx + 10] = uMax; _vertices[idx + 11] = vMax;
            _vertices[idx + 12] = e.Color.R; _vertices[idx + 13] = e.Color.G;
            _vertices[idx + 14] = e.Color.B; _vertices[idx + 15] = e.Color.A;

            _vertices[idx + 16] = x2; _vertices[idx + 17] = y2;
            _vertices[idx + 18] = uMax; _vertices[idx + 19] = vMin;
            _vertices[idx + 20] = e.Color.R; _vertices[idx + 21] = e.Color.G;
            _vertices[idx + 22] = e.Color.B; _vertices[idx + 23] = e.Color.A;

            _vertices[idx + 24] = x3; _vertices[idx + 25] = y3;
            _vertices[idx + 26] = uMin; _vertices[idx + 27] = vMin;
            _vertices[idx + 28] = e.Color.R; _vertices[idx + 29] = e.Color.G;
            _vertices[idx + 30] = e.Color.B; _vertices[idx + 31] = e.Color.A;

            _vertexCount += 4;
        }

        _dirty = false;
    }

    private void UploadToGPU()
    {
        if (_vao == -1)
        {
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            var indices = new uint[_count * 6];
            for (uint i = 0; i < (uint)_count; i++)
            {
                uint offset = i * 4;
                uint idx = i * 6;
                indices[idx + 0] = offset + 0;
                indices[idx + 1] = offset + 1;
                indices[idx + 2] = offset + 2;
                indices[idx + 3] = offset + 2;
                indices[idx + 4] = offset + 3;
                indices[idx + 5] = offset + 0;
            }
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 8 * sizeof(float), 4 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer,
            _vertexCount * 8 * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _uploadedVertices = _vertexCount;
    }

    public void Dispose()
    {
        if (_vao != -1) GL.DeleteVertexArray(_vao);
        if (_vbo != -1) GL.DeleteBuffer(_vbo);
        if (_ebo != -1) GL.DeleteBuffer(_ebo);
        _vao = _vbo = _ebo = -1;
    }
}
