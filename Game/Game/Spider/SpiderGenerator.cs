using Engine.Core.Rendering;

namespace TowerDefecse
{
    public class SpiderConfig
    {
        public Texture BodyTex { get; set; }
        public Texture LegTex { get; set; }

        public int Size { get; set; } = 32;

        public float BodyRadiusWorld { get; set; } = 0.3f;

        public int LegCount { get; set; } = 6;
        public int SegmentsPerLeg { get; set; } = 2;
        public float LegSegmentLength { get; set; } = 0.16f;
        public float FootRadialOffset { get; set; } = 0.6f;
        public float[] LegAnglesDegrees { get; set; } = { 45f, 0f, -45f, 135f, 180f, -135f };

        public float StepThreshold { get; set; } = 0.15f;
        public float SwingDuration { get; set; } = 0.22f;
        public float RotationSpeed { get; set; } = 6f;
        public float StepHeight { get; set; } = 0.08f;
        public float MaxReachFactor { get; set; } = 0.95f;
        public float ForwardFootOffset { get; set; } = 0.08f;
        public bool DebugMode { get; set; } = false;

        public SpiderConfig(Texture bodyTex, Texture legTex)
        {
            BodyTex = bodyTex;
            LegTex = legTex;
        }

        public float TotalLegLength => LegSegmentLength * SegmentsPerLeg;
    }
}
