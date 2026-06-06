using Engine.Core.GameObjects;

namespace TowerDefecse
{
    public class WaveController : Component
    {
        protected int wave = 0;
        protected float waveRate = 30;
        protected float currentTime = 0;
        public List<Enemy> enemies = new();
        public override void Start()
        {
            enemies = Enemy.AllInstances;
            Enemy.NewEnemySpawn += UpdateEnemy;
        }
        private void UpdateEnemy() => enemies = Enemy.AllInstances;
        public override void Update(float dt)
        {
            if (currentTime < waveRate) currentTime += dt;
            else
            {
                Console.WriteLine("Волна");
                currentTime = 0;
                Random rnd = new Random();
                var attackers = enemies.OrderBy(x => rnd.Next()).Take(20).ToList();
                foreach(Enemy attaker in attackers)
                {
                    attaker.isAttacking = true;
                }
            }
        }
    }
}