using OpenTK.Mathematics;

namespace Engine.Core.GameObjects;

public class Transform
{
    private Vector2 _position;
    private float _rotation;
    private Vector2 _scale = Vector2.One;

    public Transform? Parent { get; set; }
    public List<Transform> Children { get; } = new();
    public GameObject GameObject { get; internal set; } = null!;



    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public float Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    public Vector2 Scale
    {
        get => _scale;
        set => _scale = value;
    }

    public Vector2 LocalPosition
    {
        get => _position;
        set => _position = value;
    }

    public Vector2 WorldPosition
    {
        get
        {
            if (Parent == null) return _position;
            var parentMatrix = Parent.WorldMatrix;
            var localPos = Vector4.TransformRow(new Vector4(_position.X, _position.Y, 0f, 1f), parentMatrix);
            return localPos.Xy;
        }
    }

    public Matrix4 LocalMatrix =>
        Matrix4.CreateRotationZ(_rotation) *
        Matrix4.CreateScale(new Vector3(_scale.X, _scale.Y, 1f)) *
        Matrix4.CreateTranslation(new Vector3(_position.X, _position.Y, 0f));

    public Matrix4 WorldMatrix
    {
        get
        {
            if (Parent == null) return LocalMatrix;
            return LocalMatrix * Parent.WorldMatrix;
        }
    }

    public void SetParent(Transform parent)
    {
        Parent?.Children.Remove(this);
        Parent = parent;
        Parent?.Children.Add(this);
    }

    public Vector2 Right => new(MathF.Cos(_rotation), MathF.Sin(_rotation));
    public Vector2 Up => new(-MathF.Sin(_rotation), MathF.Cos(_rotation));
}
