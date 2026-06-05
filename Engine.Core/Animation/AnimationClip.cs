using OpenTK.Mathematics;
using Engine.Core.Rendering;

namespace Engine.Core.Animation;

public class AnimationClip
{
    public string Name { get; set; } = string.Empty;
    public List<AnimationFrame> Frames { get; } = new();
    public float Duration { get; set; } = 1f;
    public bool Loop { get; set; } = true;

    public Sprite? GetFrame(float time)
    {
        if (Frames.Count == 0) return null;

        var t = Loop ? time % Duration : Math.Min(time, Duration);
        var frameTime = Duration / Frames.Count;
        var index = (int)(t / frameTime);
        index = Math.Clamp(index, 0, Frames.Count - 1);

        return Frames[index].Sprite;
    }
}

public class AnimationFrame
{
    public Sprite Sprite { get; set; }
    public float Duration { get; set; }

    public AnimationFrame(Sprite sprite, float duration = 0.1f)
    {
        Sprite = sprite;
        Duration = duration;
    }
}
