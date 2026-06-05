using Engine.Core.Rendering;
using OpenTK.Mathematics;

namespace TowerDefecse
{
    public class Core : Tower
    {
        public override void Awake()
        {
            tex = new Texture("D:\\engine\\Game\\Game\\Texture\\bulletBase.png");
            ChangeSpriteBullet();
        }
        protected override void SpawnBullet(Enemy target)
        {
            var bulletGO = GameObject.Scene!.CreateGameObject($"Bullet_{GameObject.Name}_{target.GameObject.Name}");
            Vector2 direction = target.GameObject.Transform.Position - new Vector2(Transform.Position.X, Transform.Position.Y + 1.2f);
            direction.Normalize();
            var bullet = bulletGO.AddComponent<Bullet>();
            var spriteRender = bulletGO.AddComponent<SpriteRenderer>();
            spriteRender.Sprite = sprite;
            spriteRender.SortingOrder = 5;
            bullet.Transform.Position = new Vector2(Transform.Position.X, Transform.Position.Y + 1.2f);
            bullet.Transform.Rotation = MathF.Atan2(direction.Y, direction.X) - MathF.PI / 2;
            bullet.Setup(direction, attackDamage);
        }
    }
}