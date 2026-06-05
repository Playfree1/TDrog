using Engine.Core.GameObjects;

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
        private float timeSinceLastAttack = 0f;
        //----------Events---------(Доступны для подписки извне, но не для вызова)
        public static event Action<Tower, Enemy> OnAttackEnemy = delegate { };
        //----------Methods---------
        public override void Awake()
        {
            AllInstances.Add(this);
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
        protected float AttackDamageSpeed
        {
            get => attackSpeed;
            set => attackSpeed = value;
        }
    }
}