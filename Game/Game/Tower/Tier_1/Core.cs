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
        public override void Start()
        {
            AllEnemy = Enemy.AllInstances;
            tex = new Texture("D:\\engine\\Game\\Game\\Texture\\bulletBase.png");
            sprite = new Sprite(tex) { PixelsPerUnit = 32 };
            CanBeConstructed = false;
            AttackSpeed = 0.001f;
            AttackDamage = 40f;
            canAttackAir = true;
            canAttackGround = true;
        }
        protected override void SpawnBullet(Enemy target)
        {
            var bulletGO = GameObject.Scene!.CreateGameObject($"Bullet_{GameObject.Name}_{target.GameObject.Name}");
            Vector2 direction = target.GameObject.Transform.Position - new Vector2(Transform.Position.X, Transform.Position.Y + 1f);
            direction.Normalize();
            var bullet = bulletGO.AddComponent<Bullet>();
            var spriteRender = bulletGO.AddComponent<SpriteRenderer>();
            spriteRender.Sprite = sprite;
            spriteRender.SortingOrder = 5;
            bullet.Transform.Position = new Vector2(Transform.Position.X, Transform.Position.Y + 1f);
            bullet.Transform.Rotation = MathF.Atan2(direction.Y, direction.X) - MathF.PI / 2;
            bullet.Setup(direction, attackDamage);
        }
    }
}