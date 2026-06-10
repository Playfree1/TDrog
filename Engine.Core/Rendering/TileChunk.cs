using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Engine.Core.GameObjects;

namespace Engine.Core.Rendering;

public class TileChunk : Component, IDisposable
{
    public Texture? Atlas { get; set; }
    public int TileWidth { get; set; } = 32;
    public int TileHeight { get; set; } = 32;
    public float WorldTileSize { get; set; } = 1f;
    public int SortingOrder { get; set; }
    public int OriginX { get; set; }
    public int OriginY { get; set; }
    public HashSet<int> SolidTiles { get; set; } = new();

    private int _gridW, _gridH;
    private int[] _tiles = Array.Empty<int>();
    private bool _dirty = true;

    private const int VertexStride = 8;
    private float[] _vertices = Array.Empty<float>();
    private int _vertexCount;

    private int _vbo = -1;
    private int _vao = -1;
    private int _ebo = -1;
    private int _uploadedVertices;

    public void SetGrid(int width, int height)
    {
        _gridW = width;
        _gridH = height;
        _tiles = new int[width * height];
        _dirty = true;
    }

    private int ToIndex(int x, int y) => (y - OriginY) * _gridW + (x - OriginX);

    private bool InBounds(int x, int y) =>
        x >= OriginX && x < OriginX + _gridW &&
        y >= OriginY && y < OriginY + _gridH;

    public void SetTile(int x, int y, int tileIndex)
    {
        if (!InBounds(x, y)) return;
        _tiles[ToIndex(x, y)] = tileIndex;
        _dirty = true;
    }

    public void Fill(int tileIndex)
    {
        Array.Fill(_tiles, tileIndex);
        _dirty = true;
    }

    public void FillRect(int x, int y, int w, int h, int tileIndex)
    {
        for (int ty = y; ty < y + h; ty++)
            for (int tx = x; tx < x + w; tx++)
                if (InBounds(tx, ty))
                    _tiles[ToIndex(tx, ty)] = tileIndex;
        _dirty = true;
    }

    public int GetTile(int x, int y)
    {
        if (!InBounds(x, y)) return -1;
        return _tiles[ToIndex(x, y)];
    }

    public bool IsSolidAt(Vector2 worldPos) => IsSolidAt(worldPos.X, worldPos.Y);
    public bool IsSolidAt(float worldX, float worldY)
        => IsSolidAt(worldX, worldY, SolidTiles);

    public bool IsSolidAt(float worldX, float worldY, HashSet<int> solidTiles)
    {
        int gx = (int)Math.Floor(worldX / WorldTileSize);
        int gy = (int)Math.Floor(worldY / WorldTileSize);
        int idx = GetTile(gx, gy);
        return idx >= 0 && solidTiles.Contains(idx);
    }

    public bool Raycast(Vector2 origin, Vector2 direction, float maxDistance,
        out RaycastHit hit)
        => Raycast(origin, direction, maxDistance, SolidTiles, out hit);

    public bool Raycast(Vector2 origin, Vector2 direction, float maxDistance,
        HashSet<int> solidTiles, out RaycastHit hit)
    {
        hit = default;
        float len = direction.Length;
        if (len < float.Epsilon) return false;
        direction /= len;

        float ts = WorldTileSize;
        int tileX = (int)Math.Floor(origin.X / ts);
        int tileY = (int)Math.Floor(origin.Y / ts);
        int stepX = direction.X > 0 ? 1 : -1;
        int stepY = direction.Y > 0 ? 1 : -1;

        float tMaxX = direction.X != 0
            ? ((tileX + (stepX > 0 ? 1 : 0)) * ts - origin.X) / direction.X
            : float.PositiveInfinity;
        float tMaxY = direction.Y != 0
            ? ((tileY + (stepY > 0 ? 1 : 0)) * ts - origin.Y) / direction.Y
            : float.PositiveInfinity;

        float tDeltaX = direction.X != 0 ? ts / Math.Abs(direction.X) : float.PositiveInfinity;
        float tDeltaY = direction.Y != 0 ? ts / Math.Abs(direction.Y) : float.PositiveInfinity;

        int maxSteps = (int)(maxDistance / ts) + 2;
        for (int i = 0; i < maxSteps; i++)
        {
            int axis = 0;
            if (tMaxX < tMaxY) { tileX += stepX; axis = 0; }
            else               { tileY += stepY; axis = 1; }

            float dist = Math.Min(tMaxX, tMaxY);
            if (dist > maxDistance) return false;

            int idx = GetTile(tileX, tileY);
            if (idx >= 0 && solidTiles.Contains(idx))
            {
                hit = new RaycastHit(dist, idx,
                    origin + direction * dist,
                    axis == 0 ? new Vector2(-stepX, 0) : new Vector2(0, -stepY));
                return true;
            }

            if (axis == 0) tMaxX += tDeltaX;
            else           tMaxY += tDeltaY;
        }
        return false;
    }

    public override void Render()
    {
        if (Atlas == null || _tiles.Length == 0) return;

        if (_dirty)
        {
            BuildVertices();
            UploadToGPU();
        }

        if (_uploadedVertices == 0) return;

        var indexCount = _uploadedVertices / 4 * 6;
        GL.BindVertexArray(_vao);
        Renderer.DrawChunk(Atlas, indexCount);
        GL.BindVertexArray(0);
    }

    private void BuildVertices()
    {
        var atlasCols = Atlas!.Width / TileWidth;
        var total = _gridW * _gridH;

        if (_vertices.Length < total * 4 * VertexStride)
            _vertices = new float[total * 4 * VertexStride];

        _vertexCount = 0;
        var half = WorldTileSize * 0.5f;

        for (int y = 0; y < _gridH; y++)
        {
            for (int x = 0; x < _gridW; x++)
            {
                var tileIdx = _tiles[y * _gridW + x];
                if (tileIdx < 0) continue;

                var tx = tileIdx % atlasCols;
                var ty = tileIdx / atlasCols;

                // Добавляем микро-отступ (UV-inset) в 0.01 пикселя для предотвращения швов (bleeding)
                float eps = 0.01f; 
                float uMin = (tx * TileWidth + eps) / (float)Atlas.Width;
                float uMax = ((tx + 1) * TileWidth - eps) / (float)Atlas.Width;
                float vMin = (ty * TileHeight + eps) / (float)Atlas.Height;
                float vMax = ((ty + 1) * TileHeight - eps) / (float)Atlas.Height;

                float cx = (x + OriginX) * WorldTileSize + half;
                float cy = (y + OriginY) * WorldTileSize + half;
                float px0 = cx - half, py0 = cy - half;
                float px1 = cx + half, py1 = cy - half;
                float px2 = cx + half, py2 = cy + half;
                float px3 = cx - half, py3 = cy + half;

                int idx = _vertexCount * VertexStride;

                _vertices[idx + 0]  = px0; _vertices[idx + 1]  = py0;
                _vertices[idx + 2]  = uMin; _vertices[idx + 3]  = vMax;
                _vertices[idx + 4]  = 1f; _vertices[idx + 5]  = 1f;
                _vertices[idx + 6]  = 1f; _vertices[idx + 7]  = 1f;

                _vertices[idx + 8]  = px1; _vertices[idx + 9]  = py1;
                _vertices[idx + 10] = uMax; _vertices[idx + 11] = vMax;
                _vertices[idx + 12] = 1f; _vertices[idx + 13] = 1f;
                _vertices[idx + 14] = 1f; _vertices[idx + 15] = 1f;

                _vertices[idx + 16] = px2; _vertices[idx + 17] = py2;
                _vertices[idx + 18] = uMax; _vertices[idx + 19] = vMin;
                _vertices[idx + 20] = 1f; _vertices[idx + 21] = 1f;
                _vertices[idx + 22] = 1f; _vertices[idx + 23] = 1f;

                _vertices[idx + 24] = px3; _vertices[idx + 25] = py3;
                _vertices[idx + 26] = uMin; _vertices[idx + 27] = vMin;
                _vertices[idx + 28] = 1f; _vertices[idx + 29] = 1f;
                _vertices[idx + 30] = 1f; _vertices[idx + 31] = 1f;

                _vertexCount += 4;
            }
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

            var spriteCount = _vertexCount / 4;
            var indices = new uint[spriteCount * 6];
            for (uint i = 0; i < (uint)spriteCount; i++)
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

public readonly struct RaycastHit
{
    public float Distance { get; }
    public int TileIndex { get; }
    public Vector2 Point { get; }
    public Vector2 Normal { get; }

    public RaycastHit(float distance, int tileIndex, Vector2 point, Vector2 normal)
    {
        Distance = distance;
        TileIndex = tileIndex;
        Point = point;
        Normal = normal;
    }
}
