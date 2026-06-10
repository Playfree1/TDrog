using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Engine.Core.Rendering;
using OpenTK.Mathematics;

namespace TowerDefecse
{
    public class Cannon : Tower
    {
        protected override void Setup()
        {
            gunTexture = new Texture("D:\\engine\\Game\\Game\\Texture\\CoreGun.png");
            sprite = new Sprite(tex) { PixelsPerUnit = 32 };
            CanBeConstructed = true;
            AttackSpeed = 0.85f;
            AttackDamage = 15f;
            MaxHp = 50;
            CurrHP = 50;
            canAttackAir = false;
            canAttackGround = true;
            ChangeSprite();
        }
        protected override void SpawnBullet(Enemy target)
        {
            var bulletGO = GameObject.Scene!.CreateGameObject($"Bullet_{GameObject.Name}_{target.GameObject.Transform.Position}");
            Vector2 direction = target.GameObject.Transform.Position - new Vector2(Transform.Position.X, Transform.Position.Y);
            direction.Normalize();
            var bullet = bulletGO.AddComponent<Bullet>();
            var spriteRender = bulletGO.AddComponent<SpriteRenderer>();
            spriteRender.Sprite = sprite;
            spriteRender.SortingOrder = 5;
            float barrelLength = 0.5f;

            // Рассчитываем точку смещения от центра пушки в сторону выстрела
            Vector2 spawnPosition = new Vector2(Transform.Position.X, Transform.Position.Y) + direction * barrelLength;

            bullet.Transform.Position = spawnPosition;
            bullet.Transform.Rotation = MathF.Atan2(direction.Y, direction.X) - MathF.PI / 2;
            bullet.Setup(direction, attackDamage);
        }
    }
}