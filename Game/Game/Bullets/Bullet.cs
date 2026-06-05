using Engine.Core.GameObjects;
using Engine.Core.Rendering;
using Engine.Core.Physics;
using OpenTK.Mathematics;

namespace TowerDefecse;

public class Bullet : Component
{
    private Vector2 _direction;
    private float _speed = 20f;
    private float _damage = 25f;
    private TileChunk _tiles = null!;

    public void Setup(Vector2 direction, float damage)
    {
        _direction = direction;
        _damage = damage;
    }

    public override void Start()
    {
        _tiles = GameObject.Scene!.FindObjectOfType<TileChunk>()!;
    }

    public override void FixedUpdate(float dt)
    {
        Transform.Position += _direction * _speed * dt;

        if (_tiles.IsSolidAt(Transform.Position.X, Transform.Position.Y))
        {
            GameObject.Destroy();
            return;
        }

        foreach (var enemy in Enemy.AllInstances)
        {
            if (enemy == null) continue;
            float distSq = (enemy.Transform.Position - Transform.Position).LengthSquared;
            if (distSq > 4f) continue; // дальше 2 единиц — пропускаем

            if (Collision.Overlaps(GameObject, enemy.GameObject))
            {
                enemy.ApplyDamage(_damage);
                GameObject.Destroy();
                return;
            }
        }
    }
}
