using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Engine.Core;
using Engine.Core.Rendering;
using Engine.Core.GameObjects;
using Engine.Core.Input;
using Engine.Core.Scene;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Minesweeper;

class Cell : GameObject
{
    public int GridX, GridY;
    public bool IsMine;
    public bool IsRevealed;
    public bool IsFlagged;
    public int AdjacentMines;
    public SpriteRenderer Renderer;

    public Cell(Scene scene, int x, int y) : base($"Cell_{x}_{y}")
    {
        GridX = x;
        GridY = y;
        Transform.Position = new Vector2(x, y);
        Transform.Scale = new Vector2(0.92f, 0.92f);
        Renderer = AddComponent<SpriteRenderer>();
        Renderer.SortingOrder = 0;
        scene.AddGameObject(this);
    }
}

public class MinesweeperGame : Game
{
    private const int GridSize = 10;
    private const int MineCount = 20;

    private Scene _scene = null!;
    private Camera _camera = null!;
    private Cell[,] _cells = new Cell[GridSize, GridSize];
    private Sprite _cellSprite = null!;
    private bool _gameOver;
    private bool _victory;
    private int _revealedCount;
    private bool _firstClick = true;

    private static readonly Color4[] NumberColors =
    {
        new(0.75f, 0.75f, 0.75f, 1f), // 0
        new(0.2f, 0.4f, 1.0f, 1f),    // 1
        new(0.2f, 0.8f, 0.2f, 1f),    // 2
        new(0.55f, 0.2f, 0.5f, 1f),    // 3
        new(0.1f, 0.1f, 0.6f, 1f),    // 4
        new(0.6f, 0.1f, 0.1f, 1f),    // 5
        new(0.1f, 0.6f, 0.6f, 1f),    // 6
        new(0.2f, 0.2f, 0.2f, 1f),    // 7
        new(0.5f, 0.5f, 0.5f, 1f),    // 8
    };

    private static readonly Color4 HiddenColor = new(0.5f, 0.5f, 0.5f, 1f);
    private static readonly Color4 FlagColor = new(1.0f, 0.3f, 0.3f, 1f);
    private static readonly Color4 MineColor = new(0.1f, 0.1f, 0.1f, 1f);
    private static readonly Color4 ExplodedColor = new(1.0f, 0.0f, 0.0f, 1f);
    private static readonly Color4 WinHiddenMine = new(0.0f, 0.6f, 0.0f, 1f);

    public MinesweeperGame(GameWindowSettings gs, NativeWindowSettings ns) : base(gs, ns) { }

    protected override void OnLoad()
    {
        base.OnLoad();

        _camera = new Camera
        {
            Position = new Vector2(10f, 10f),
            Width = 25f,
            Zoom = 1f
        };
        Renderer.MainCamera = _camera;

        _scene = new Scene("Minesweeper");

        var tex = Texture.CreateCheckerboard(32, 0xFFFFFFFF, 0xFFFFFFFF);
        _cellSprite = new Sprite(tex) { PixelsPerUnit = 32 };

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                var cell = new Cell(_scene, x, y);
                cell.Renderer.Sprite = _cellSprite;
                cell.Renderer.Color = HiddenColor;
                cell.Renderer.SortingOrder = 0;
                _cells[x, y] = cell;
            }
        }
        if(MineCount >= GridSize * GridSize)
            throw new Exception("Too many mines!");
        _scene.Load();
        LoadScene(_scene);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        if (Input.GetMouseButtonDown(1)) HandleRightClick();

        if (!_gameOver && Input.GetKeyDown(Keys.R))
            Restart();

        if (_gameOver && Input.GetKeyDown(Keys.Space))
            Restart();

        var mouse = Input.MousePosition;
        var world = _camera.ScreenToWorld(mouse);
        Title = _victory ? "Minesweeper - YOU WIN! (SPACE to restart)"
             : _gameOver ? "Minesweeper - GAME OVER (SPACE to restart)"
             : $"Minesweeper - {MineCount - FlagCount()} flags left (R to restart)";
        Title += $" | mouse=({mouse.X:F0},{mouse.Y:F0}) world=({world.X:F2},{world.Y:F2})";
    }

    private void GetGridClick(out int gx, out int gy)
    {
        var worldPos = _camera.ScreenToWorld(Input.MousePosition);
        gx = (int)Math.Round(worldPos.X, MidpointRounding.AwayFromZero);
        gy = (int)Math.Round(worldPos.Y, MidpointRounding.AwayFromZero);
    }

    private int FlagCount()
    {
        int count = 0;
        for (int y = 0; y < GridSize; y++)
            for (int x = 0; x < GridSize; x++)
                if (_cells[x, y].IsFlagged) count++;
        return count;
    }

    private void HandleLeftClick()
    {
        if (_gameOver) return;

        GetGridClick(out int gx, out int gy);
        if (gx < 0 || gx >= GridSize || gy < 0 || gy >= GridSize) return;

        var cell = _cells[gx, gy];
        if (cell.IsRevealed || cell.IsFlagged) return;

        if (_firstClick)
        {
            _firstClick = false;
            SetupMines(gx, gy);
        }

        RevealCell(gx, gy);
    }

    private void HandleRightClick()
    {
        if (_gameOver) return;

        GetGridClick(out int gx, out int gy);
        if (gx < 0 || gx >= GridSize || gy < 0 || gy >= GridSize) return;

        var cell = _cells[gx, gy];
        if (cell.IsRevealed) return;

        cell.IsFlagged = !cell.IsFlagged;
        cell.Renderer.Color = cell.IsFlagged ? FlagColor : HiddenColor;
    }

    private void SetupMines(int safeX, int safeY)
    {
        var rng = new Random();
        int placed = 0;
        while (placed < MineCount)
        {
            int x = rng.Next(GridSize);
            int y = rng.Next(GridSize);
            if (_cells[x, y].IsMine) continue;
            if (Math.Abs(x - safeX) <= 1 && Math.Abs(y - safeY) <= 1) continue;

            _cells[x, y].IsMine = true;
            placed++;
        }

        for (int y = 0; y < GridSize; y++)
            for (int x = 0; x < GridSize; x++)
                if (!_cells[x, y].IsMine)
                    _cells[x, y].AdjacentMines = CountAdjacentMines(x, y);
    }

    private int CountAdjacentMines(int x, int y)
    {
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < GridSize && ny >= 0 && ny < GridSize && _cells[nx, ny].IsMine)
                    count++;
            }
        return count;
    }

    private void RevealCell(int x, int y)
    {
        var cell = _cells[x, y];
        if (cell.IsRevealed || cell.IsFlagged) return;

        cell.IsRevealed = true;
        _revealedCount++;

        if (cell.IsMine)
        {
            GameOver(cell);
            return;
        }

        cell.Renderer.Color = NumberColors[cell.AdjacentMines];

        if (cell.AdjacentMines == 0)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < GridSize && ny >= 0 && ny < GridSize)
                        RevealCell(nx, ny);
                }
        }

        if (_revealedCount >= GridSize * GridSize - MineCount)
            Win();
    }

    private void GameOver(Cell hitMine)
    {
        _gameOver = true;
        hitMine.Renderer.Color = ExplodedColor;

        for (int y = 0; y < GridSize; y++)
            for (int x = 0; x < GridSize; x++)
            {
                var c = _cells[x, y];
                if (c.IsMine && !c.IsRevealed)
                    c.Renderer.Color = MineColor;
            }
    }

    private void Win()
    {
        _gameOver = true;
        _victory = true;

        for (int y = 0; y < GridSize; y++)
            for (int x = 0; x < GridSize; x++)
            {
                var c = _cells[x, y];
                if (c.IsMine && !c.IsRevealed)
                    c.Renderer.Color = WinHiddenMine;
            }
    }

    private void Restart()
    {
        _gameOver = false;
        _victory = false;
        _firstClick = true;
        _revealedCount = 0;

        for (int y = 0; y < GridSize; y++)
            for (int x = 0; x < GridSize; x++)
            {
                var c = _cells[x, y];
                c.IsMine = false;
                c.IsRevealed = false;
                c.IsFlagged = false;
                c.AdjacentMines = 0;
                c.Renderer.Color = HiddenColor;
            }
    }
}
