using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using Engine.Core;
using Engine.Core.Rendering;
using Engine.Core.GameObjects;
using Engine.Core.Scene;
using OpenTK.Windowing.Common;
using Engine.Core.Mathematic;
using Engine.Core.Pathfinding;
using Engine.Core.Input;



namespace TowerDefecse
{

    public class StartGame : Game
    {
        private GameObject go = null!;

        private Scene _scene = null!;
        private Camera _camera = null!;
        private SpriteRenderer spriteRenderer = null!;
        public TileChunk _chunk = null!;
        private FlowFields? _flowFields;
        private bool _showDebug = true;
        public StartGame(GameWindowSettings gs, NativeWindowSettings ns) : base(gs, ns) { }
        protected override void OnLoad()
        {
            base.OnLoad();
            _camera = new Camera
            {
                Position = new Vector2(200f, 200f),
                Width = 10f,
                Zoom = 1f
            };
            Renderer.MainCamera = _camera;
            _scene = new Scene("Test Scene");

            var terrainTiles = GenerateTerrain();
            var core = SpawnPlayer(terrainTiles);

            _flowFields = new FlowFields();
            _flowFields.Setup(_chunk, 400, core);
            SpawnEnemy(terrainTiles, _flowFields);
            var waveObj = _scene.CreateGameObject("WaveController");
            waveObj.AddComponent<WaveController>();
            _scene.Load();
            LoadScene(_scene);
            GC.Collect();
        }
        float time = 0f;
        private GameObject player = null!;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            time += (float)args.Time;
            if (Math.Abs(time) > 1f)
            {
                time = 0f;
                Title = $"FPS: {1f / args.Time:F2} | Upd: {Game.LastUpdateUs}us | Rend: {Game.LastRenderUs}us";
            }
            if (Input.GetKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.F3))
                _showDebug = !_showDebug;
        }
        private Vector2 SpawnPlayer(int[,] terrainTiles)
        {
            go = _scene.CreateGameObject("Player");
            var core = _scene.CreateGameObject("Core");
            var tex = new Texture("D:\\engine\\Game\\Game\\Texture\\Player.png");
            var sprite = new Sprite(tex) { PixelsPerUnit = 32 };
            spriteRenderer = go.AddComponent<SpriteRenderer>();
            var mover = go.AddComponent<Mover>();
            mover.chunk = _chunk;
            var cameraMover = go.AddComponent<CameraMover>();
            var attacker = go.AddComponent<Attacker>();
            attacker.camera = _camera;
            go.AddComponent<Player>();
            cameraMover.camera = _camera;
            cameraMover.player = mover;
            player = go;
            Vector2 safeSpot = FindClosesestSafeSpot(terrainTiles);
            _camera.Position = safeSpot;
            go.Transform.Position = safeSpot;
            go.Transform.Scale = new Vector2(1, 1);
            spriteRenderer.Sprite = sprite;
            spriteRenderer.SortingOrder = 10;
            //Core
            var coreCom = core.AddComponent<Core>();
            core.Transform.Position = new Vector2(safeSpot.X + 0.5f, safeSpot.Y + 0.5f);
            var spriteRendererCore = core.AddComponent<SpriteRenderer>();
            var texCore = new Texture("D:\\engine\\Game\\Game\\Texture\\Core.png");
            var spriteCore = new Sprite(texCore) { PixelsPerUnit = 32 };
            spriteRendererCore.SortingOrder = 2;
            spriteRendererCore.Sprite = spriteCore;
            return safeSpot;
        }

        private Vector2 FindClosesestSafeSpot(int[,] terrainTiles)
        {
            int centerX = 200;
            int centerY = 200;
            for (int radius = 9; radius < 50; radius++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    for (int y = centerY - radius; y <= centerY + radius; y++)
                    {
                        if (IsSafeSpot(terrainTiles, x, y, 3))
                        {
                            return new Vector2(x + 0.5f, y + 0.5f);
                        }
                    }
                }
            }
            return new Vector2(centerX, centerY);
        }
        private bool IsSafeSpot(int[,] terrainTiles, int xC, int yC, int radius)
        {
            for (int y = yC - radius; y <= yC + radius; y++)
            {
                for (int x = xC - radius; x <= xC + radius; x++)
                {
                    if (terrainTiles[x, y] == 1) return false;
                }
            }
            return true;
        }

        private int[,] GenerateTerrain()
        {
            var builder = new AtlasBuilder(32, 32);
            int grassId = builder.AddFromFile("grass", @"D:\engine\Game\Game\Texture\grass.png");
            int wallId = builder.AddFromFile("wall", @"D:\engine\Game\Game\Texture\wall.png");
            int poisenedGrass = builder.AddFromFile("poisenedGrass", @"D:\engine\Game\Game\Texture\grassPoisened.png");
            var atlas = builder.Build();
            var random = new Random();
            var chunk = _scene.CreateGameObject("Terrain").AddComponent<TileChunk>();
            chunk.SolidTiles.Add(wallId);
            chunk.Atlas = atlas.Atlas;
            chunk.SetGrid(400, 400);
            float offsetX = random.Next(0, 1000000) * 0.1f;
            float offsetY = random.Next(0, 1000000) * 0.1f;
            float centerX = 200f;
            float centerY = 200f;
            float maxDistance = 700f;
            int[,] tiles = new int[400, 400];

            PerlinNoise perlin = new PerlinNoise(random.Next(0, 1000000));
            for (int y = 0; y < 400; y++)
                for (int x = 0; x < 400; x++)
                {
                    float sampleX = x * 0.1f + offsetX;
                    float sampleY = y * 0.1f + offsetY;
                    float noise = perlin.Noise(sampleX, sampleY, 5, 0.3f);
                    float dx = x - centerX;
                    float dy = y - centerY;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float distanceFactor = distance / maxDistance;
                    distanceFactor = Math.Clamp(distanceFactor, 0f, 1f);
                    distanceFactor = MathF.Pow(distanceFactor, 3f); // Square to create a smoother falloff
                    float adjustedNoise = noise - distanceFactor;
                    // 1. Сначала жестко отделяем стены от зоны проходимого пола
                    if (adjustedNoise < 0.4935f)
                    {
                        tiles[x, y] = wallId;
                    }
                    else
                    {
                        // 2. Если мы внутри пола, проверяем, насколько "глубокая" это зона.
                        // Чем выше adjustedNoise, тем дальше этот тайл от стен (центр поляны).
                        if (adjustedNoise > 0.7f) // Настройте этот порог (например, 0.7f - 0.8f)
                        {
                            tiles[x, y] = poisenedGrass;
                        }
                        else
                        {
                            tiles[x, y] = grassId;
                        }
                    }
                }
            bool[,] visited = new bool[400, 400];
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            int startX = 200;
            int startY = 200;
            if (tiles[startX, startY] == wallId)
            {
                for (int d = 1; d < 200; d++)
                {
                    if (startX + d < 400 && tiles[startX + d, startY] != wallId) { startX += d; break; }
                    if (startY + d < 400 && tiles[startX, startY + d] != wallId) { startY += d; break; }
                    if (startX - d >= 0 && tiles[startX - d, startY] != wallId) { startX -= d; break; }
                    if (startY - d >= 0 && tiles[startX, startY - d] != wallId) { startY -= d; break; }
                }
            }
            if (tiles[startX, startY] != wallId)
            {
                queue.Enqueue((startX, startY));
                visited[startX, startY] = true;

                int[] dx4 = { 1, -1, 0, 0 };
                int[] dy4 = { 0, 0, 1, -1 };

                while (queue.Count > 0)
                {
                    var curr = queue.Dequeue();
                    for (int i = 0; i < 4; i++)
                    {
                        int nx = curr.x + dx4[i];
                        int ny = curr.y + dy4[i];
                        if (nx >= 0 && nx < 400 && ny >= 0 && ny < 400 && !visited[nx, ny] && tiles[nx, ny] != wallId)
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }
            }
            for (int y = 0; y < 400; y++)
                for (int x = 0; x < 400; x++)
                    if (!visited[x, y])
                    {
                        tiles[x, y] = wallId;
                    }

            for (int y = 0; y < 400; y++)
                for (int x = 0; x < 400; x++)
                    chunk.SetTile(x, y, tiles[x, y]);

            _chunk = chunk;
            return tiles;
        }
        private void SpawnEnemy(int[,] terrainTiles, FlowFields flowFields)
        {
            var tex = new Texture("D:\\engine\\Game\\Game\\Texture\\Enemy.png");
            var sprite = new Sprite(tex) { PixelsPerUnit = 32 };
            var rng = new Random();
            for (int x = 1; x < 399; x++)
                for (int y = 1; y < 399; y++)
                    if (IsSafeSpot(terrainTiles, x, y, 1) && rng.Next(0, 100) < 9 && (new Vector2(x, y) - new Vector2(200, 200)).Length > 20f)
                    {
                        var go = _scene.CreateGameObject($"Enemy_{x}_{y}");
                        var spriteRenderer = go.AddComponent<SpriteRenderer>();
                        var enemy = go.AddComponent<Enemy>();
                        enemy.flowField = flowFields;
                        go.Transform.Position = new Vector2(x, y);
                        go.Transform.Scale = new Vector2(1, 1);
                        spriteRenderer.Sprite = sprite;
                        spriteRenderer.SortingOrder = 5;
                    }

        }
        protected override void OnAfterRender()
        {
            //_flowFields?.DrawDebug(_camera);
            base.OnAfterRender();
        }
    }
}
