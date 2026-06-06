using Engine.Core.GameObjects;
using Engine.Core.Rendering;
using OpenTK.Mathematics;

namespace TowerDefecse
{
    public class Tower : Component
    {
        //----------Static---------
        public static List<Tower> AllInstances { get; } = new();
        //----------Fields---------
        protected float attackRange = 5f;
        protected float attackDamage = 20f;
        protected float attackSpeed = 1f;
        protected int woodCost = 1;
        protected int rockCost = 0;
        protected int ironCost = 0;
        protected bool canBeConstructed = true;
        protected bool canAttackAir = false;
        protected bool canAttackGround = true;
        protected List<Enemy> AllEnemy = new();
        protected List<Enemy> enemiesInRange = new();
        protected Texture tex = null!;
        protected Sprite sprite = null!;
        protected Texture gunTexture = null!;
        protected GameObject weaponGO = null!;

        private float timeSinceLastAttack = 0f;
        //----------Events---------(Доступны для подписки извне, но не для вызова)
        public static event Action<Tower, Enemy> OnAttackEnemy = delegate { };
        public static event Action NewTowerBuild = delegate { };
        //----------Methods---------
        public override void Awake()
        {
            AllInstances.Add(this);
            NewTowerBuild.Invoke();
            
        }
        public override void Start()
        {
            AllEnemy = Enemy.AllInstances;
            tex = new Texture("D:\\engine\\Game\\Game\\Texture\\bulletBase.png");
            sprite = new Sprite(tex) { PixelsPerUnit = 32 };
            Setup();
            SpawnGun();
        }
        protected virtual void Setup() { }
        protected virtual void SpawnGun()
        {
            if (gunTexture == null) return;
            weaponGO = GameObject.CreateChild("Weapon");
            var _weaponRenderer = weaponGO.AddComponent<SpriteRenderer>();
            var gunSprite = new Sprite(gunTexture) { PixelsPerUnit = 32 };
            _weaponRenderer.Sprite = gunSprite;
            _weaponRenderer.SortingOrder = 7;
        }
        protected void ChangeSprite()
        {
            sprite = new Sprite(tex) { PixelsPerUnit = 32 };
        }
        public override void FixedUpdate(float dt)
        {
            // Чистим мёртвых врагов из списка
            for (int i = enemiesInRange.Count - 1; i >= 0; i--)
                if (!enemiesInRange[i].Enabled)
                    enemiesInRange.RemoveAt(i);

            if (timeSinceLastAttack <= attackSpeed)
            {
                timeSinceLastAttack += dt;
            }
            else
            {
                //Console.WriteLine(enemiesInRange.Count);
                if (enemiesInRange.Count == 0)
                {
                    GetTarget();
                }
                else
                {
                    if (RotateGun())
                    {
                        Attack();
                        timeSinceLastAttack = 0f;
                    }
                }
            }
        }
        protected bool RotateGun()
        {
            var target = enemiesInRange[0];
            Vector2 dir = target.Transform.Position - Transform.Position;
            weaponGO.Transform.Rotation = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2;
            return true;
        }
        protected virtual void GetTarget()
        {
            foreach (var enemy in AllEnemy)
            {
                if (attackRange <= 0) break;
                if ((enemy.Transform.Position - Transform.Position).Length <= attackRange)
                {
                    if (!enemiesInRange.Contains(enemy))
                    {
                        enemiesInRange.Add(enemy);
                    }
                }
                else
                {
                    if (enemiesInRange.Contains(enemy))
                    {
                        enemiesInRange.Remove(enemy);
                    }
                }
            }
        }
        protected virtual void Attack()
        {
            var target = enemiesInRange[0];
            SpawnBullet(target);
        }
        protected virtual void SpawnBullet(Enemy target)
        {
            var bulletGO = GameObject.Scene!.CreateGameObject($"Bullet_{GameObject.Name}_{target.GameObject.Name}");
            Vector2 direction = target.GameObject.Transform.Position - GameObject.Transform.Position;
            direction.Normalize();
            var bullet = bulletGO.AddComponent<Bullet>();
            var spriteRender = bulletGO.AddComponent<SpriteRenderer>();
            spriteRender.Sprite = sprite;
            spriteRender.SortingOrder = 5;
            bullet.Transform.Position = new Vector2(Transform.Position.X, Transform.Position.Y);
            bullet.Transform.Rotation = MathF.Atan2(direction.Y, direction.X) - MathF.PI / 2;
            bullet.Setup(direction, attackDamage);
        }
        //----------Properties---------
        protected float AttackRange
        {
            get => attackRange;
            set => attackRange = value;
        }
        protected float AttackDamage
        {
            get => attackDamage;
            set => attackDamage = value;
        }
        protected float AttackSpeed
        {
            get => attackSpeed;
            set => attackSpeed = value;
        }
        protected int WoodCost
        {
            get => woodCost;
            set => woodCost = value;
        }
        public int RockCost
        {
            get => rockCost;
            set => rockCost = value;
        }
        public int IronCost
        {
            get => ironCost;
            set => ironCost = value;
        }
        public bool CanBeConstructed
        {
            get => canBeConstructed;
            set => canBeConstructed = value;
        }
        public bool CanAttackAir
        {
            get => canAttackAir;
            set => canAttackAir = value;
        }
        public bool CanAttackGround
        {
            get => canAttackGround;
            set => canAttackGround = value;
        }
        public Texture Texture
        {
            get => tex;
            set => tex = value;
        }
    }
}