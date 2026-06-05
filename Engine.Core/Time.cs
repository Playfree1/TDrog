namespace Engine.Core;

public static class Time
{
    public static float DeltaTime { get; internal set; }
    public static float FixedDeltaTime { get; set; } = 1f / 60f;
    public static float TimeScale { get; set; } = 1f;
    public static float UnscaledDeltaTime { get; internal set; }
    public static float ElapsedTime { get; internal set; }
    public static int FrameCount { get; internal set; }
    public static float FixedDeltaTimeAccumulator { get; internal set; }
}
