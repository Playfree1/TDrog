namespace Engine.Core.Scene;

public class Scene
{
    public string Name { get; set; }
    public bool IsLoaded { get; private set; }

    private readonly List<GameObjects.GameObject> _gameObjects = new();
    private readonly List<GameObjects.GameObject> _pendingAdd = new();
    private readonly List<GameObjects.GameObject> _pendingRemove = new();
    private bool _isUpdating;

    public Scene(string name = "Scene")
    {
        Name = name;
    }

    public GameObjects.GameObject CreateGameObject(string name = "GameObject")
    {
        var go = new GameObjects.GameObject(name);
        go.Scene = this;
        _pendingAdd.Add(go);
        return go;
    }

    public void AddGameObject(GameObjects.GameObject go)
    {
        go.Scene = this;
        _pendingAdd.Add(go);
    }

    internal void RemoveGameObject(GameObjects.GameObject go)
    {
        if (_isUpdating)
            _pendingRemove.Add(go);
        else
            _gameObjects.Remove(go);
    }

    public void Load()
    {
        ProcessPending();
        IsLoaded = true;
    }

    public void Unload()
    {
        var copy = _gameObjects.ToArray();
        foreach (var go in copy)
            go.Destroy();
        _gameObjects.Clear();
        _pendingAdd.Clear();
        _pendingRemove.Clear();
    }

    public void Update(float dt)
    {
        if (!IsLoaded) return;
        _isUpdating = true;

        foreach (var go in _gameObjects)
            go.Update(dt);

        _isUpdating = false;
        ProcessPending();
    }

    public void FixedUpdate(float dt)
    {
        if (!IsLoaded) return;
        _isUpdating = true;

        foreach (var go in _gameObjects)
            go.FixedUpdate(dt);

        _isUpdating = false;
        ProcessPending();
    }

    public void Render()
    {
        if (!IsLoaded) return;

        _isUpdating = true;
        foreach (var go in _gameObjects)
            go.Render();
        _isUpdating = false;
    }

    private void ProcessPending()
    {
        if (_pendingAdd.Count > 0)
        {
            foreach (var go in _pendingAdd)
            {
                _gameObjects.Add(go);
                go.Start();
            }
            _pendingAdd.Clear();
        }

        if (_pendingRemove.Count > 0)
        {
            foreach (var go in _pendingRemove)
                _gameObjects.Remove(go);
            _pendingRemove.Clear();
        }
    }

    public T? FindObjectOfType<T>() where T : GameObjects.Component
    {
        foreach (var go in _gameObjects)
        {
            var comp = go.GetComponent<T>();
            if (comp != null) return comp;
        }
        return null;
    }

    public List<T> FindObjectsOfType<T>() where T : GameObjects.Component
    {
        var result = new List<T>();
        foreach (var go in _gameObjects)
        {
            var comps = go.GetComponents<T>();
            result.AddRange(comps);
        }
        return result;
    }

    public GameObjects.GameObject? FindGameObject(string name)
    {
        foreach (var go in _gameObjects)
            if (go.Name == name) return go;
        return null;
    }
}
