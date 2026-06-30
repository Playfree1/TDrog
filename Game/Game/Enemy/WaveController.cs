using Engine.Core.GameObjects;
using Engine.Core.Pathfinding;
using Engine.Core.Rendering;
using OpenTK.Mathematics;

namespace TowerDefecse
{
    public class WaveController : Component
    {
        protected int wave = 0;
        protected float waveRate = 30;
        protected float currentTime = 0;
        private int attackersCount = 5;
        private int newAttakers = 3;
        private List<Enemy> enemies = new();
        private List<Enemy> attackers = new();
        public FlowFields[] targetTurret = new FlowFields[3]{ new(), new(), new() };
        private FlowFields[] backgroundTurrets = new FlowFields[3] { new(), new(), new() };
        private TileChunk chunk = null!;
        private List<Tower> tower = null!;
        private Dictionary<Tower, Vector2> position = new();
        Random rnd = null!;
        private bool isUpdatingFields = false;
        public override void Start()
        {
            rnd = new Random();
            chunk = GameObject.Scene!.FindObjectOfType<TileChunk>()!;
            UpdateEnemy();
            Enemy.EnemyCountChange += UpdateEnemy;
            Tower.TowerDestroyed += UpdateTarget;
            Tower.NewTowerBuild += UpdateTarget;
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
                attackers = enemies.OrderBy(x => rnd.Next()).Take(attackersCount).ToList();
                attackersCount += newAttakers;
                currentTime = 0;
                foreach (Enemy attaker in attackers)
                {
                    attaker.isAttacking = true;
                    attaker.mayAttackPlayerEvenIfAttakers = rnd.Next(10) == 1;
                }
            }
        }
        private async void UpdateTarget(Vector2 target)
        {
            if (isUpdatingFields) return;

            tower = Tower.AllInstances;
            if (tower.Count <= 3) return;
            tower = tower.OrderBy(x => rnd.Next()).ToList();

            isUpdatingFields = true;


            var targetPositions = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                var firstTower = tower.First();
                targetPositions[i] = firstTower.Transform.Position;
                tower.Remove(firstTower);
            }
            await Task.Run(() =>
            {
                for (int i = 0; i < 3; i++)
                    backgroundTurrets[i].Setup(chunk,400,targetPositions[i]);
            });
            var temp = targetTurret;
            targetTurret = backgroundTurrets;
            backgroundTurrets = temp;
            isUpdatingFields = false;
        }
    }
}