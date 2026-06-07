using Engine.Core.GameObjects;

namespace TowerDefecse
{
    public class WaveController : Component
    {
        protected int wave = 0;
        protected float waveRate = 30;
        protected float currentTime = -30;
        private int attackersCount = 15;
        private List<Enemy> enemies = new();
        private List<Enemy> attackers = new();
        public override void Start()
        {
            UpdateEnemy();
            Enemy.EnemyCountChange += UpdateEnemy;
        }
        private void UpdateEnemy()
        {
            enemies = Enemy.AllInstances;
        }
        public override void Update(float dt)
        {
            if (currentTime < waveRate) currentTime += dt;
            else
            {
                Random rnd = new Random();
                attackers = enemies.OrderBy(x => rnd.Next()).Take(attackersCount).ToList();
                attackersCount += 5;
                currentTime = 0;
                foreach (Enemy attaker in attackers)
                {
                    attaker.isAttacking = true;
                }
            }
        }
    }
}