namespace Engine.Core.GameObjects;

public class GameObject
{
    public string Name { get; set; }
    public Transform Transform { get; }
    public Scene.Scene? Scene { get; internal set; }
    public bool Active { get; set; } = true;

    private readonly List<Component> _components = new();
    private bool _started;
    private bool _isDestroyed;

    public GameObject(string name = "GameObject")
    {
        Name = name;
        Transform = new Transform();
    }

    public T AddComponent<T>() where T : Component, new()
    {
        var component = new T();
        component.GameObject = this;
        component.Enabled = true;
        _components.Add(component);
        component.Awake();

        if (_started)
            component.Start();

        return component;
    }

    public T GetComponent<T>() where T : Component
    {
        foreach (var c in _components)
            if (c is T result)
                return result;
        return null!;
    }

    public T? GetComponentInChildren<T>() where T : Component
    {
        var comp = GetComponent<T>();
        if (comp != null) return comp;

        foreach (var child in Transform.Children)
            if (child is T result)
                return result;
        return null!;
    }

    public T[] GetComponents<T>() where T : Component
    {
        return _components.OfType<T>().ToArray();
    }

    internal void Start()
    {
        if (_started) return;
        _started = true;

        for (int i = _components.Count - 1; i >= 0; i--)
        {
            if (_isDestroyed) break;
            if (_components[i].Enabled)
                _components[i].Start();
        }
    }

    internal void Update(float dt)
    {
        if (!Active || _isDestroyed) return;

        for (int i = _components.Count - 1; i >= 0; i--)
        {
            if (_isDestroyed) break;
            if (_components[i].Enabled)
                _components[i].Update(dt);
        }
    }

    internal void FixedUpdate(float dt)
    {
        if (!Active || _isDestroyed) return;

        for (int i = _components.Count - 1; i >= 0; i--)
        {
            if (_isDestroyed) break;
            if (_components[i].Enabled)
                _components[i].FixedUpdate(dt);
        }
    }

    internal void Render()
    {
        if (!Active || _isDestroyed) return;

        for (int i = _components.Count - 1; i >= 0; i--)
        {
            if (_isDestroyed) break;
            if (_components[i].Enabled)
                _components[i].Render();
        }
    }

    public void Destroy()
    {
        if (_isDestroyed) return;
        _isDestroyed = true;
        Active = false;

        for (int i = _components.Count - 1; i >= 0; i--)
        {
            _components[i].Enabled = false;
            _components[i].OnDestroy();
        }

        Scene?.RemoveGameObject(this);
    }
}
