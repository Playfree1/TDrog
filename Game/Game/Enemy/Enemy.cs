using Engine.Core.GameObjects;
using Engine.Core;
using TowerDefecse;
using Engine.Core.Rendering;
using OpenTK.Mathematics;
using Engine.Core.Physics;
using Engine.Core.Pathfinding;
using Engine.Core.Mathematic;
using OpenTK.Graphics.ES11;


namespace TowerDefecse;

public class Enemy : Component
{
    //----------Static---------
    public static List<Enemy> AllInstances { get; } = new();
    //----------Fields---------
    protected float speed = 4f;
    protected float maxHealth = 100f;
    protected float currentHealth = 100f;
    protected float radiusOfView = 10f;
    protected float attackRange = 1f;
    protected float attackSpeed = 1;
    protected float attackDamage = 10f;
    protected float touchDamage = 5f;
    protected float armoring = 0f;
    protected List<Tower> towers = null!;
    private GameObject player = null!;
    private TileChunk tiles = null!;
    protected Vector2 LastKnownPlayerPosition;
    protected FlowFields flowFields = null!;
    protected RandomF randomF = null!;


    //----------Events---------(Доступны для подписки извне, но не для вызова)
    public static event Action EnemySeePlayer = delegate { };
    public static event Action<float> EnemyTakeDamage = delegate { };
    public static event Action EnemyDie = delegate { };
    public static event Action<float> OnHitPlayer = delegate { };
    public static event Action EnemyAttack = delegate { };




    //----------Methods---------
    public override void Awake()
    {
        AllInstances.Add(this);
    }
    public override void Start()
    {
        randomF = new RandomF();
        player = Player.Instance!.GameObject;
        tiles = GameObject.Scene!.FindObjectOfType<TileChunk>()!;
        speed = speed * randomF.Range(0.85f, 1.25f);
        Tower.NewTowerBuild += UpdateTower;
    }
    private void UpdateTower()
    {
        towers = Tower.AllInstances;
    }
    public override void Update(float dt)
    {
        PassiveBehavior();
        if (TrySeePlayer())
        {
            LastKnownPlayerPosition = player.Transform.Position;
            EnemySeePlayer.Invoke();
            if ((Transform.Position - player.Transform.Position).Length <= attackRange)
            {
                Attack();
                if (Collision.Overlaps(GameObject, player))
                    EnemyTouchPlayer();
            }
            else
            {
                MoverBehavior();
            }
        }
        else
        {
            MoveToLastKnownPosition();
        }
    }
    /// <summary>
    /// Пассивное поведение врага, выполняемое каждый кадр, даже если он не видит игрока. Например, патрулирование или стояние на месте.
    /// Не Использовать без проверок чтобы не перегрузить игру, например if(Enemy.RadiusOfView > (LastKnownPlayerPosition - Transform.Position).Length)
    /// </summary>
    protected virtual void PassiveBehavior() { }
    protected virtual void MoveToLastKnownPosition()
    {
        if (LastKnownPlayerPosition != Vector2.Zero)
        {
            if ((Transform.Position - LastKnownPlayerPosition).Length <= 0.1f) { LastKnownPlayerPosition = Vector2.Zero; return; } // Уже на месте
            Vector2 direction = (LastKnownPlayerPosition - Transform.Position).Normalized();
            Transform.Position += direction * speed * Time.DeltaTime;
        }
        else
        {
            int tileX = (int)Math.Floor(Transform.Position.X);
            int tileY = (int)Math.Floor(Transform.Position.Y);
            Vector2 currentTile = new Vector2(tileX, tileY);
            if (flowFields.GetFlowField().TryGetValue(currentTile, out Vector2 moveDirection))
            {
                // Если вектор нулевой (пришли к цели или стоим в стене), не двигаемся
                if (moveDirection != Vector2.Zero)
                {
                    // 2. Находим точный МИРОВОЙ ЦЕНТР текущего тайла
                    Vector2 tileCenter = new Vector2(tileX + 0.5f, tileY + 0.5f);

                    // 3. Вычисляем вектор от врага к центру его текущего тайла
                    Vector2 toCenter = tileCenter - Transform.Position;

                    // 4. Смешиваем направление из Flow Field и силу подтягивания к центру.
                    // Коэффициент 0.35f определяет, насколько сильно врага "магнитит" к центру дороги,
                    // чтобы он не срезал углы стен слишком сильно. Поэкспериментируйте с ним.
                    Vector2 finalDirection = moveDirection + (toCenter * randomF.Range(0.35f,0.95f));

                    // 5. Нормализуем итоговый вектор, чтобы скорость всегда была стабильной
                    if (finalDirection.LengthSquared > 0)
                    {
                        finalDirection = finalDirection.Normalized();
                    }
                    Transform.Position += finalDirection * speed * Time.DeltaTime;
                }
            }
        }
    }
    /// <summary>
    /// Активное поведение врага, выполняемое, когда он видит игрока и в недостаточной дальности чтобы атаковать его.
    /// </summary>
    protected virtual void MoverBehavior()
    {
        Vector2 direction = (player.Transform.Position - Transform.Position).Normalized();
        Transform.Position += direction * speed * Time.DeltaTime;
    }
    /// <summary>
    /// определяет видит ли враг игрока, базово кидает луч от врага к игроку и проверяет что он ничего не задел.
    /// </summary>
    /// <returns>Обязательное возвращение результата</returns>
    protected virtual bool TrySeePlayer()
    {
        Vector2 directionToPlayer = player.Transform.Position - Transform.Position;
        float distanceToPlayer = directionToPlayer.Length;
        if (distanceToPlayer > radiusOfView) return false; // Игрок вне радиуса видимости
        directionToPlayer /= distanceToPlayer; // Нормализуем направление

        if (tiles.Raycast(Transform.Position, directionToPlayer, distanceToPlayer, out var hit)) // Радиус видимости
        {
            // Проверяем, есть ли препятствия между врагом и игроком
            if (hit.TileIndex >= 0 && tiles.SolidTiles.Contains(hit.TileIndex))
            {
                return false; // Враг не видит игрока из-за препятствия
            }
        }
        return true;
    }
    protected virtual void Attack()
    {
        EnemyAttack.Invoke();
    }
    protected virtual void EnemyTouchPlayer()
    {
        OnHitPlayer.Invoke(Armor(TouchDamage) * Time.DeltaTime);
    }
    public override void OnDestroy()
    {
        AllInstances.Remove(this);
    }
    public void ApplyDamage(float damage)
    {
        TakeDamage(Armor(damage));
    }
    protected virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        EnemyTakeDamage.Invoke(damage);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    protected virtual void Die()
    {
        EnemyDie.Invoke();
        GameObject.Destroy();
    }
    protected virtual float Armor(float damage)
    {
        return damage * (1 - armoring);
    }


    //----------Properties---------
    protected float Speed
    {
        get => speed;
        set => speed = value;
    }
    protected float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }
    protected float CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = value;
    }
    protected float RadiusOfView
    {
        get => radiusOfView;
        set => radiusOfView = value;
    }
    protected float AttackRange
    {
        get => attackRange;
        set => attackRange = value;
    }
    protected float AttackSpeed
    {
        get => attackSpeed;
        set => attackSpeed = value;
    }
    protected float AttackDamage
    {
        get => attackDamage;
        set => attackDamage = value;
    }
    protected float TouchDamage
    {
        get => touchDamage;
        set => touchDamage = value;
    }
    protected float Armoring
    {
        get => armoring;
        set => armoring = value;
    }
    public FlowFields flowField
    {
        get => flowFields;
        set => flowFields = value;
    }
}