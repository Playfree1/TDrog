using Engine.Core;
using Engine.Core.GameObjects;
using Engine.Core.Physics;
using Engine.Core.Rendering;
using Engine.Core.UI;
using OpenTK.Graphics.ES11;
using System.Windows;

namespace TowerDefecse
{
    public class Player : Component
    {
        //------------Instance------------
        public static Player? Instance { get; private set; }
        public override void Awake()
        {
            Instance = this;
        }
        //------------Fields------------
        protected float Speed { get; set; } = 4.5f;
        protected float maxHealth = 100f;
        protected float currentHealth = 100f;

        private const int HeartCount = 1;
        private const int HeartSize = 32;
        private Texture _heartSheet = null!;
        //------------Events------------
        protected event Action<float> PlayerTakeDamage = delegate { };
        //------------Methods------------
        public override void Start()
        {
            Enemy.OnHitPlayer += TakeDamage;
            _heartSheet = new Texture("D:\\engine\\Game\\Game\\Texture\\HPPlayer.png");
            Game.OnDrawUI += DrawUI;

        }
        public override void FixedUpdate(float dt)
        {
            foreach (var enemy in Enemy.AllInstances)
            {
                if (enemy == null) continue;
                if (enemy.CanAttack() == false) continue;
                float distSq = (enemy.Transform.Position - Transform.Position).LengthSquared;
                if (distSq > 4f) continue; // дальше 2 единиц — пропускаем

                if (Collision.Overlaps(GameObject, enemy.GameObject))
                {
                    TakeDamage(enemy.GetDamage());
                    enemy.SuccesfulAttack();
                }
            }
        }
        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            PlayerTakeDamage.Invoke(damage);
            if (currentHealth <= 0)
                Die();
        }
        private void DrawUI(Canvas canvas)
        {
            float healthFrac = currentHealth / maxHealth;
            int frame = (int)((1f - healthFrac) * 4f);
            frame = Math.Clamp(frame, 0, 4);

            float heartSize = 0.09f * Renderer.ScreenHeight; // 9% от высоты экрана
            float srcSize = 32f; // размер региона в спрайт-листе

            float x = 0f;
            float y = 1f * Renderer.ScreenHeight - heartSize;

            canvas.DrawTextureRegion(_heartSheet,
                x, y,
                heartSize, heartSize,
                frame * srcSize, 0, srcSize, srcSize);
        }
        public override void OnDestroy()
        {
            Enemy.OnHitPlayer -= TakeDamage;
            Game.OnDrawUI -= DrawUI;
        }
        public virtual void Die()
        {
            GameObject.Destroy();
        }
    }
}