using Engine.Core.GameObjects;
using Engine.Core.Rendering;
using OpenTK.Mathematics;

namespace Engine.Core.Physics;

public static class Collision
{
    public static bool Overlaps(GameObject a, GameObject b)
    {
        var halfA = GetHalfSize(a);
        var halfB = GetHalfSize(b);
        return Overlaps(a.Transform.Position, halfA, b.Transform.Position, halfB);
    }

    public static bool Overlaps(Vector2 posA, Vector2 halfA, Vector2 posB, Vector2 halfB)
    {
        return Math.Abs(posA.X - posB.X) < halfA.X + halfB.X &&
               Math.Abs(posA.Y - posB.Y) < halfA.Y + halfB.Y;
    }

    public static bool OverlapsPoint(Vector2 posA, Vector2 halfA, Vector2 point)
    {
        return Math.Abs(posA.X - point.X) < halfA.X &&
               Math.Abs(posA.Y - point.Y) < halfA.Y;
    }

    public static Vector2 GetHalfSize(GameObject go)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr?.Sprite == null)
            return Vector2.Zero;

        var worldSize = sr.Sprite.Size / sr.Sprite.PixelsPerUnit * go.Transform.Scale;
        return worldSize * 0.5f;
    }
}
