using OpenTK.Mathematics;
using Engine.Core.Rendering;
using Engine.Core.UI;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Core.Pathfinding
{
    public class FlowFields
    {
        private Dictionary<Vector2, int> tiles = new Dictionary<Vector2, int>();
        private Dictionary<Vector2, Vector2> direction = new Dictionary<Vector2, Vector2>();
        private int wallId = 1;
        private Canvas _debugCanvas = new();
        private BitmapFont _defaultFont = null!;
        private int[,] grid = null!;
        private bool[,] visited = null!;
        private bool _arraysInitialized = false;


        private void InitializeArrays(int mapsize)
        {
            if (_arraysInitialized && grid.GetLength(0) == mapsize) return;

            grid = new int[mapsize, mapsize];
            visited = new bool[mapsize, mapsize];
            _arraysInitialized = true;
        }

        public void Setup(TileChunk tileChunk, int mapsize, Vector2 CorePosition)
        {
            InitializeArrays(mapsize);

            tiles.Clear();
            direction.Clear();
            Array.Clear(visited, 0, visited.Length); 

            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            for (int x = 0; x < mapsize; x++)
            {
                for (int y = 0; y < mapsize; y++)
                {
                    grid[x, y] = tileChunk.GetTile(x, y);
                }
            }

            int startX = (int)CorePosition.X;
            int startY = (int)CorePosition.Y;

            if (grid[startX, startY] == wallId)
            {
                for (int d = 1; d < mapsize / 2; d++)
                {
                    if (startX + d < mapsize && grid[startX + d, startY] != wallId) { startX += d; break; }
                    if (startY + d < mapsize && grid[startX, startY + d] != wallId) { startY += d; break; }
                    if (startX - d >= 0 && grid[startX - d, startY] != wallId) { startX -= d; break; }
                    if (startY - d >= 0 && grid[startX, startY - d] != wallId) { startY -= d; break; }
                }
            }

            if (grid[startX, startY] != wallId)
            {
                queue.Enqueue((startX, startY));
                grid[startX, startY] = 0;
                visited[startX, startY] = true;

                Vector2 startKey = new Vector2(startX, startY);
                this.tiles[startKey] = 0;

                int[] dx4 = { 1, -1, 0, 0 };
                int[] dy4 = { 0, 0, 1, -1 };

                while (queue.Count > 0)
                {
                    var curr = queue.Dequeue();
                    Vector2 currKey = new Vector2(curr.x, curr.y);

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = curr.x + dx4[i];
                        int ny = curr.y + dy4[i];

                        if (nx >= 0 && nx < mapsize && ny >= 0 && ny < mapsize && !visited[nx, ny] && grid[nx, ny] != wallId)
                        {
                            visited[nx, ny] = true;
                            Vector2 nextKey = new Vector2(nx, ny);
                            tiles[nextKey] = tiles[currKey] + 1;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }

                // Arrow chunk — 1 GameObject, 1 draw call
                //var arrowObj = _scene.CreateGameObject("ArrowChunk");
                //var arrowChunk = arrowObj.AddComponent<SpriteChunk>();
                //var tex = new Texture("D:\\engine\\Game\\Game\\Texture\\str.png");
                //arrowChunk.Sprite = new Sprite(tex) { PixelsPerUnit = 32 };
                //arrowChunk.SortingOrder = 4;

                for (int x = 0; x < mapsize; x++)
                    for (int y = 0; y < mapsize; y++)
                    {
                        if (!tiles.TryGetValue(new Vector2(x, y), out _)) continue;
                        Vector2 curr = new Vector2(x, y);
                        Vector2 nextCell = curr;
                        int lastMin = int.MaxValue;
                        for (int i = 0; i < 4; i++)
                        {
                            int nx = (int)curr.X + dx4[i];
                            int ny = (int)curr.Y + dy4[i];
                            if (!tiles.TryGetValue(new Vector2(nx, ny), out int o)) continue;
                            int currentMin = tiles[new Vector2(nx, ny)];
                            if (currentMin < lastMin)
                            {
                                lastMin = currentMin;
                                nextCell = new Vector2(nx, ny);
                            }
                        }

                        Vector2 direct = nextCell - curr;
                        direction[curr] = direct;
                        //float angle = MathF.Atan2(direct.Y, direct.X) - MathF.PI / 2;
                        //if (x == 0 && y == 0) continue;
                        //arrowChunk.Add(new Vector2(x + 0.5f, y + 0.5f), null, null, angle);
                    }
            }
        }
        public IReadOnlyDictionary<Vector2, Vector2> GetFlowField()
        {
            return direction;
        }

        public void DrawDebug(Camera camera)
        {
            GL.Disable(EnableCap.DepthTest);

            // ВАЖНО: Передаем игровую камеру в Begin, чтобы Canvas рисовал в мировых координатах
            _debugCanvas.Begin(camera);

            _defaultFont ??= BitmapFont.CreateDefault(); // Если переменная недоступна, создайте поле в классе

            // Границы видимости камеры
            int minX = (int)Math.Floor(camera.ViewLeft);
            int maxX = (int)Math.Ceiling(camera.ViewRight);
            int minY = (int)Math.Floor(camera.ViewBottom);
            int maxY = (int)Math.Ceiling(camera.ViewTop);

            foreach (var pair in tiles)
            {
                Vector2 pos = pair.Key;

                if (pos.X >= minX && pos.X <= maxX && pos.Y >= minY && pos.Y <= maxY)
                {
                    // Передаем точный центр тайла (pos.X + 0.5f, pos.Y + 0.5f)
                    // И уменьшаем масштаб (scale), так как в мировых координатах 1 единица = 1 тайл.
                    // Шрифту нужно поставить очень маленький scale (например, 0.02f или 0.03f), 
                    // чтобы буквы не были размером с 20 тайлов.
                    _debugCanvas.DrawWorldTextCentered(
                        _defaultFont,
                        pair.Value.ToString(),
                        new Vector2(pos.X + 0.5f, pos.Y + 0.5f),
                        Color4.White,
                        0.04f
                    );
                }
            }
            _debugCanvas.End();
        }
    }
}
