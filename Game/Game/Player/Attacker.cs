using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Engine.Core.GameObjects;
using Engine.Core.Input;
using Engine.Core.Rendering;
using Engine.Core.Scene;
using OpenTK.Mathematics;

namespace TowerDefecse
{
    public class Attacker : Component
    {
        protected float attackSpeed = 0.7f;
        private float attackCooldown = 0f;
        public Camera camera = null!;
        private Texture tex = null!;
        private Sprite sprite = null!;
        SpawnTurret spawnTurret = null!;
        public override void Start()
        {
            tex = new Texture("D:\\engine\\Game\\Game\\Texture\\BulletBase.png");
            sprite = new Sprite(tex) { PixelsPerUnit = 32 };
            spawnTurret = new SpawnTurret();
            spawnTurret.scene = GameObject.Scene!;
        }

        public override void Update(float dt)
        {

            if (attackCooldown <= 0f)
            {
                if (Input.GetMouseButton(0))
                {
                    Attack();
                    attackCooldown = attackSpeed;
                }
            }
            else attackCooldown -= dt;
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 worldPos = camera.ScreenToWorld(Input.MousePosition);
                if(Tower.IsEnoughResources<Cannon>())
                spawnTurret.SpawnTower(worldPos,SpawnTurret.TurretType.Cannon);
            }
        }
        protected virtual void Attack()
        {
            Vector2 worldPos = camera.ScreenToWorld(Input.MousePosition);
            Vector2 direction = worldPos - GameObject.Transform.Position;
            direction.Normalize();
            var bullet = GameObject.Scene!.CreateGameObject("Bullet");
            bullet.Transform.Position = Transform.Position;
            bullet.Transform.Rotation = MathF.Atan2(direction.Y, direction.X) - MathF.PI / 2;
            var spriteRenderer = bullet.AddComponent<SpriteRenderer>();
            spriteRenderer.Sprite = sprite;
            spriteRenderer.SortingOrder = 10;
            var bulletComponent = bullet.AddComponent<Bullet>();
            bulletComponent.Setup(direction, 20f);
        }
    }
}