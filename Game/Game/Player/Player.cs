using Engine.Core.GameObjects;

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
        //------------Events------------
        protected event Action<float> PlayerTakeDamage = delegate { };
        //------------Methods------------
        public override void Start()
        {
            Enemy.OnHitPlayer += TakeDamage;
        }
        public override void Update(float dt)
        {
            
        }
        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            PlayerTakeDamage.Invoke(damage);
            if (currentHealth <= 0)
                Die();
        }
        public override void OnDestroy()
        {
            Enemy.OnHitPlayer -= TakeDamage;
        }
        public virtual void Die()
        {
            GameObject.Destroy();
        }
    }
}