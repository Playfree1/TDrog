using Engine.Core.GameObjects;

namespace Engine.Core.Animation;

public class Animator : Component
{
    private readonly Dictionary<string, AnimationClip> _clips = new();
    private readonly Dictionary<string, float> _floatParams = new();
    private readonly Dictionary<string, int> _intParams = new();
    private readonly Dictionary<string, bool> _boolParams = new();
    private readonly HashSet<string> _triggers = new();

    private AnimationClip? _currentClip;
    private float _currentTime;
    private string? _currentState;

    public string? CurrentState => _currentState;
    public AnimationClip? CurrentClip => _currentClip;
    public bool IsPlaying { get; private set; }

    public event Action<string>? OnAnimationEnd;
    public event Action<string>? OnStateChanged;

    public void AddClip(AnimationClip clip)
    {
        _clips[clip.Name] = clip;
    }

    public void Play(string stateName)
    {
        if (!_clips.TryGetValue(stateName, out var clip)) return;

        if (_currentState != stateName)
        {
            _currentState = stateName;
            _currentTime = 0f;
            _currentClip = clip;
            IsPlaying = true;
            OnStateChanged?.Invoke(stateName);
        }
    }

    public void SetFloat(string name, float value) => _floatParams[name] = value;
    public void SetInt(string name, int value) => _intParams[name] = value;
    public void SetBool(string name, bool value) => _boolParams[name] = value;
    public void SetTrigger(string name) => _triggers.Add(name);

    public float GetFloat(string name) => _floatParams.GetValueOrDefault(name);
    public int GetInt(string name) => _intParams.GetValueOrDefault(name);
    public bool GetBool(string name) => _boolParams.GetValueOrDefault(name);
    public bool GetTrigger(string name) => _triggers.Contains(name);

    public void ResetTrigger(string name) => _triggers.Remove(name);

    public override void Update(float deltaTime)
    {
        if (!IsPlaying || _currentClip == null || _currentClip.Frames.Count == 0) return;

        _currentTime += deltaTime;

        if (_currentTime >= _currentClip.Duration)
        {
            OnAnimationEnd?.Invoke(_currentState!);

            if (_currentClip.Loop)
                _currentTime %= _currentClip.Duration;
            else
            {
                _currentTime = _currentClip.Duration;
                IsPlaying = false;
            }
        }
    }

    public Rendering.Sprite? GetCurrentSprite()
    {
        return _currentClip?.GetFrame(_currentTime);
    }
}
