namespace Engine.Core.GameObjects;

public abstract class Component
{
    public GameObject GameObject { get; internal set; } = null!;
    public Transform Transform => GameObject.Transform;
    public bool Enabled { get; set; } = true;

    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update(float deltaTime) { }
    public virtual void FixedUpdate(float fixedDeltaTime) { }
    public virtual void Render() { }
    public virtual void OnDestroy() { }

    public T GetComponent<T>() where T : Component => GameObject.GetComponent<T>();
    public T? GetComponentInChildren<T>() where T : Component => GameObject.GetComponentInChildren<T>();
}
