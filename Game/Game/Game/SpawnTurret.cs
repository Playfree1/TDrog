
using Engine.Core.GameObjects;
using Engine.Core.Rendering;
using Engine.Core.Scene;
using OpenTK.Mathematics;

namespace TowerDefecse
{
    public class SpawnTurret : Component
    {
        private List<Vector2> turretPosition = new List<Vector2>();
        public Scene scene = null!;
        private bool Initialized = false;
        private TileChunk tileChunk = null!;
        public enum TurretType
        {
            Cannon = 0,
        }
        public void SpawnTower(Vector2 spawnPosition, TurretType Type)
        {
            Inst();
            spawnPosition = new Vector2(MathF.Floor(spawnPosition.X), MathF.Floor(spawnPosition.Y));
            if (turretPosition.Contains(spawnPosition)) return;
            turretPosition.Add(spawnPosition);
            var tower = scene.CreateGameObject(spawnPosition.ToString());
            if(tower == null) return;
            if(tileChunk.IsSolidAt(spawnPosition) || turretPosition.Count > 100) return;
            switch (Type)
            {
                case TurretType.Cannon:
                    var coreCom = tower.AddComponent<Cannon>();
                    tower.Transform.Position = new Vector2(spawnPosition.X + 0.5f, spawnPosition.Y + 0.5f);
                    var spriteRendererCore = tower.AddComponent<SpriteRenderer>();
                    var texCore = new Texture("D:\\engine\\Game\\Game\\Texture\\Base.png");
                    var spriteCore = new Sprite(texCore) { PixelsPerUnit = 32 };
                    spriteRendererCore.SortingOrder = 2;
                    spriteRendererCore.Sprite = spriteCore;
                    break;
            }
        }
        private void Inst()
        {
            if (Initialized) return;
            Tower.TowerDestroyed += RemoveTowerFrowList;
            tileChunk = scene.FindObjectOfType<TileChunk>()!;
            Initialized = true;
        }
        private void RemoveTowerFrowList(Vector2 tower)
        {
            tower = new Vector2(MathF.Floor(tower.X), MathF.Floor(tower.Y));
            if(turretPosition.Contains(tower)) turretPosition.Remove(tower);
        }
    }
}