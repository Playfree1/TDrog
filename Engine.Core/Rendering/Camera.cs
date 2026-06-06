using OpenTK.Mathematics;

namespace Engine.Core.Rendering;

public class Camera
{
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public float Zoom { get; set; } = 1f;

    public float Width { get; set; } = 16f;

    public float ViewHeight => Width / Zoom;
    public float ViewWidth => ViewHeight * (Renderer.ScreenWidth / (float)Renderer.ScreenHeight);
    public float ViewLeft => Position.X - ViewWidth * 0.5f;
    public float ViewRight => Position.X + ViewWidth * 0.5f;
    public float ViewBottom => Position.Y - ViewHeight * 0.5f;
    public float ViewTop => Position.Y + ViewHeight * 0.5f;

    public bool IsVisible(Vector2 worldPos, Vector2 halfSize)
    {
        return worldPos.X + halfSize.X >= ViewLeft &&
               worldPos.X - halfSize.X <= ViewRight &&
               worldPos.Y + halfSize.Y >= ViewBottom &&
               worldPos.Y - halfSize.Y <= ViewTop;
    }
    public Matrix4 ViewMatrix => Matrix4.LookAt(
        new Vector3(Position.X, Position.Y, 1f),
        new Vector3(Position.X, Position.Y, 0f),
        Vector3.UnitY
    );

    public Matrix4 ProjectionMatrix
    {
        get
        {
            var aspect = Renderer.ScreenWidth / (float)Renderer.ScreenHeight;
            var h = Width / Zoom;
            var w = h * aspect;
            return Matrix4.CreateOrthographic(w, h, 0.01f, 100f);
        }
    }

    public Matrix4 ViewProjectionMatrix => ViewMatrix * ProjectionMatrix;

    public Matrix4 ViewProjectionMatrixInverse => Matrix4.Invert(ViewProjectionMatrix);

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        var aspect = Renderer.ScreenWidth / (float)Renderer.ScreenHeight;
        var h = Width / Zoom;
        var w = h * aspect;

        var ndcX = screenPos.X / Renderer.ScreenWidth * 2f - 1f;
        var ndcY = 1f - screenPos.Y / Renderer.ScreenHeight * 2f;

        return new Vector2(
            Position.X + ndcX * w * 0.5f,
            Position.Y + ndcY * h * 0.5f
        );
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        var aspect = Renderer.ScreenWidth / (float)Renderer.ScreenHeight;
        var h = Width / Zoom;
        var w = h * aspect;

        float ndcX = (worldPos.X - Position.X) / (w * 0.5f);
        float ndcY = (worldPos.Y - Position.Y) / (h * 0.5f);

        return new Vector2(
            (ndcX + 1f) * Renderer.ScreenWidth * 0.5f,
            (1f - ndcY) * Renderer.ScreenHeight * 0.5f
        );
    }
}
