using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Core.Rendering;

internal struct SpriteDrawData
{
    public Sprite Sprite;
    public Vector2 Position;
    public Vector2 Scale;
    public float Rotation;
    public Color4 Color;
    public Vector2 Origin;
    public float Depth;
}

public class SpriteBatch : IDisposable
{
    private const int VertexStride = 8;

    private readonly int _maxBatchSize;
    private readonly int _maxVertices;
    private readonly int _maxIndices;

    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    internal readonly Shader _shader;

    private readonly List<SpriteDrawData> _sprites;
    private readonly float[] _vertices;
    private readonly uint[] _indices;

    private Matrix4 _viewProjection;
    private int _vertexCount;
    private int _indexCount;

    private static readonly string VertexShaderSource = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 aColor;

out vec2 vTexCoord;
out vec4 vColor;

uniform mat4 uViewProjection;

void main()
{
    gl_Position = uViewProjection * vec4(aPosition, 0.0, 1.0);
    vTexCoord = aTexCoord;
    vColor = aColor;
}";

    private static readonly string FragmentShaderSource = @"
#version 330 core
in vec2 vTexCoord;
in vec4 vColor;

out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, vTexCoord) * vColor;
}";

    private class SpriteComparer : IComparer<SpriteDrawData>
    {
        public int Compare(SpriteDrawData x, SpriteDrawData y)
        {
            int depthCompare = x.Depth.CompareTo(y.Depth);
            if (depthCompare != 0) return depthCompare;
            return x.Sprite.Texture.Handle.CompareTo(y.Sprite.Texture.Handle);
        }
    }

    private static readonly SpriteComparer ComparerInstance = new();

    public SpriteBatch(int maxBatchSize = 16384)
    {
        _maxBatchSize = maxBatchSize;
        _maxVertices = maxBatchSize * 4;
        _maxIndices = maxBatchSize * 6;

        _sprites = new List<SpriteDrawData>(maxBatchSize);
        _vertices = new float[_maxVertices * VertexStride];
        _indices = new uint[_maxIndices];

        for (uint i = 0; i < maxBatchSize; i++)
        {
            uint offset = i * 4;
            uint idx = i * 6;
            _indices[idx + 0] = offset + 0;
            _indices[idx + 1] = offset + 1;
            _indices[idx + 2] = offset + 2;
            _indices[idx + 3] = offset + 2;
            _indices[idx + 4] = offset + 3;
            _indices[idx + 5] = offset + 0;
        }

        _shader = new Shader(VertexShaderSource, FragmentShaderSource);
        _shader.Use();
        _shader.SetInt("uTexture", 0);

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _maxVertices * VertexStride * sizeof(float),
            IntPtr.Zero, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, VertexStride * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VertexStride * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, VertexStride * sizeof(float), 4 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _maxIndices * sizeof(uint),
            _indices, BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public void Begin(Camera camera)
    {
        _viewProjection = camera.ViewProjectionMatrix;
        _sprites.Clear();
        _vertexCount = 0;
        _indexCount = 0;
    }

    public void Draw(Sprite sprite, Vector2 position, Vector2 scale, float rotation,
        Color4 color, Vector2 origin, float depth)
    {
        if (_sprites.Count >= _maxBatchSize)
            FlushCollected();

        _sprites.Add(new SpriteDrawData
        {
            Sprite = sprite,
            Position = position,
            Scale = scale,
            Rotation = rotation,
            Color = color,
            Origin = origin,
            Depth = depth
        });
    }

    public int DebugSpriteCount => _sprites.Count;

    public void End()
    {
        FlushCollected();
    }

    private void FlushCollected()
    {
        if (_sprites.Count == 0) return;

        _sprites.Sort(ComparerInstance);

        int i = 0;
        while (i < _sprites.Count)
        {
            var tex = _sprites[i].Sprite.Texture;
            int start = i;
            while (i < _sprites.Count && _sprites[i].Sprite.Texture.Handle == tex.Handle)
            {
                i++;
            }
            FlushGroupRange(tex, start, i);
        }

        _sprites.Clear();
    }

    private void FlushGroupRange(Texture texture, int start, int end)
    {
        if (start >= end) return;

        _vertexCount = 0;
        _indexCount = 0;

        for (int i = start; i < end; i++)
        {
            if (_vertexCount + 4 > _maxVertices)
            {
                SubmitVertices(texture);
                _vertexCount = 0;
                _indexCount = 0;
            }

            var data = _sprites[i];
            var w = data.Sprite.Size.X / data.Sprite.PixelsPerUnit;
            var h = data.Sprite.Size.Y / data.Sprite.PixelsPerUnit;
            var ox = data.Origin.X * w;
            var oy = data.Origin.Y * h;

            float cos = 1f;
            float sin = 0f;
            bool hasRotation = data.Rotation != 0f;
            if (hasRotation)
            {
                cos = MathF.Cos(data.Rotation);
                sin = MathF.Sin(data.Rotation);
            }

            var src = data.Sprite.SourceRect!.Value;
            var texW = data.Sprite.Texture.Width;
            var texH = data.Sprite.Texture.Height;

            float uMin = src.Min.X / texW;
            float uMax = src.Max.X / texW;
            float vMin = src.Min.Y / texH;
            float vMax = src.Max.Y / texH;

            float x0 = -ox;
            float y0 = -oy;
            float x1 = w - ox;
            float y1 = -oy;
            float x2 = w - ox;
            float y2 = h - oy;
            float x3 = -ox;
            float y3 = h - oy;

            float px0, py0, px1, py1, px2, py2, px3, py3;

            if (hasRotation)
            {
                px0 = data.Position.X + (x0 * cos - y0 * sin) * data.Scale.X;
                py0 = data.Position.Y + (x0 * sin + y0 * cos) * data.Scale.Y;
                px1 = data.Position.X + (x1 * cos - y1 * sin) * data.Scale.X;
                py1 = data.Position.Y + (x1 * sin + y1 * cos) * data.Scale.Y;
                px2 = data.Position.X + (x2 * cos - y2 * sin) * data.Scale.X;
                py2 = data.Position.Y + (x2 * sin + y2 * cos) * data.Scale.Y;
                px3 = data.Position.X + (x3 * cos - y3 * sin) * data.Scale.X;
                py3 = data.Position.Y + (x3 * sin + y3 * cos) * data.Scale.Y;
            }
            else
            {
                px0 = data.Position.X + x0 * data.Scale.X;
                py0 = data.Position.Y + y0 * data.Scale.Y;
                px1 = data.Position.X + x1 * data.Scale.X;
                py1 = data.Position.Y + y1 * data.Scale.Y;
                px2 = data.Position.X + x2 * data.Scale.X;
                py2 = data.Position.Y + y2 * data.Scale.Y;
                px3 = data.Position.X + x3 * data.Scale.X;
                py3 = data.Position.Y + y3 * data.Scale.Y;
            }

            int idx = _vertexCount * VertexStride;

            _vertices[idx + 0] = px0;
            _vertices[idx + 1] = py0;
            _vertices[idx + 2] = uMin;
            _vertices[idx + 3] = vMax;
            _vertices[idx + 4] = data.Color.R;
            _vertices[idx + 5] = data.Color.G;
            _vertices[idx + 6] = data.Color.B;
            _vertices[idx + 7] = data.Color.A;

            _vertices[idx + 8] = px1;
            _vertices[idx + 9] = py1;
            _vertices[idx + 10] = uMax;
            _vertices[idx + 11] = vMax;
            _vertices[idx + 12] = data.Color.R;
            _vertices[idx + 13] = data.Color.G;
            _vertices[idx + 14] = data.Color.B;
            _vertices[idx + 15] = data.Color.A;

            _vertices[idx + 16] = px2;
            _vertices[idx + 17] = py2;
            _vertices[idx + 18] = uMax;
            _vertices[idx + 19] = vMin;
            _vertices[idx + 20] = data.Color.R;
            _vertices[idx + 21] = data.Color.G;
            _vertices[idx + 22] = data.Color.B;
            _vertices[idx + 23] = data.Color.A;

            _vertices[idx + 24] = px3;
            _vertices[idx + 25] = py3;
            _vertices[idx + 26] = uMin;
            _vertices[idx + 27] = vMin;
            _vertices[idx + 28] = data.Color.R;
            _vertices[idx + 29] = data.Color.G;
            _vertices[idx + 30] = data.Color.B;
            _vertices[idx + 31] = data.Color.A;

            _vertexCount += 4;
            _indexCount = _vertexCount / 4 * 6;
        }

        if (_vertexCount > 0)
            SubmitVertices(texture);
    }

    private void SubmitVertices(Texture texture)
    {
        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero,
            _vertexCount * VertexStride * sizeof(float), _vertices);

        _shader.Use();
        _shader.SetMatrix4("uViewProjection", _viewProjection);

        texture.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
    }

    public void Dispose()
    {
        _shader.Dispose();
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteVertexArray(_vao);
    }
}
