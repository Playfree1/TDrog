using Engine.Core.GameObjects;
using Engine.Core.Rendering;
using OpenTK.Mathematics;

namespace TowerDefecse
{
    public class SpiderLegAnimator : Component
    {
        public SpiderConfig Config { get; set; } = null!;

        private static readonly Box2 UpperSrc = new(0, 0, 5, 5);
        private static readonly Box2 LowerSrc = new(0, 5, 5, 11);

        private Mover _mover = null!;
        private LegData[] _legs = null!;
        private List<GameObject> _legVisuals = new();
        private int _activeGroup = -1;
        private Texture? _debugTex;
        private GameObject[]? _debugTargets;
        private GameObject[]? _debugFoots;

        private struct LegData
        {
            public int Group;
            public int Side;
            public int Idx;
            public Vector2 AnchorLocal;
            public Vector2 TargetLocal;
            public Vector2 FootWorld;
            public bool Swinging;
            public float SwingProgress;
            public float SwingPhaseOffset;
            public Vector2 SwingStart;
            public Vector2 SwingEnd;
            public GameObject[] Segments;
            public float KneeDir;
            public bool HasSwungThisCycle;
        }

        public override void Start()
        {
            _mover = GetComponent<Mover>()!;

            var bodyRenderer = GetComponent<SpriteRenderer>()!;
            bodyRenderer.Sprite = new Sprite(Config.BodyTex) { PixelsPerUnit = 32 };

            InitializeLegs();

            if (Config.DebugMode)
                CreateDebugVisuals();
        }

        private void CreateDebugVisuals()
        {
            var whiteData = new byte[8 * 8 * 4];
            for (int i = 0; i < whiteData.Length; i++) whiteData[i] = 255;
            _debugTex = new Texture(8, 8, whiteData);

            _debugTargets = new GameObject[Config.LegCount];
            _debugFoots = new GameObject[Config.LegCount];
            for (int i = 0; i < Config.LegCount; i++)
            {
                var t = new GameObject($"DebugTarget_{i}");
                GameObject.Scene?.AddGameObject(t);
                var tr = t.AddComponent<SpriteRenderer>();
                tr.Sprite = new Sprite(_debugTex) { PixelsPerUnit = 32 };
                tr.Color = new Color4(1f, 0f, 0f, 1f);
                tr.SortingOrder = 20;
                _debugTargets[i] = t;

                var f = new GameObject($"DebugFoot_{i}");
                GameObject.Scene?.AddGameObject(f);
                var fr = f.AddComponent<SpriteRenderer>();
                fr.Sprite = new Sprite(_debugTex) { PixelsPerUnit = 32 };
                fr.Color = new Color4(0f, 1f, 0f, 1f);
                fr.SortingOrder = 20;
                _debugFoots[i] = f;
            }
        }

        private void InitializeLegs()
        {
            _legs = new LegData[Config.LegCount];

            for (int idx = 0; idx < Config.LegCount; idx++)
            {
                int side = idx < Config.LegCount / 2 ? 0 : 1;
                int legsPerSide = Config.LegCount / 2;
                int i = idx - side * legsPerSide;
                float t = legsPerSide > 1 ? (float)i / (legsPerSide - 1) : 0.5f;

                float baseSideAngle = side == 0 ? 0f : MathF.PI;
                float sdir = side == 0 ? 1f : -1f;
                float attachAngle = baseSideAngle + sdir * 0.1f * (1f - 2f * t);

                Vector2 anchor = new Vector2(
                    MathF.Cos(attachAngle) * Config.BodyRadiusWorld,
                    MathF.Sin(attachAngle) * Config.BodyRadiusWorld);

                float legAngleDeg = Config.LegAnglesDegrees[idx];
                float legAngleRad = legAngleDeg * MathF.PI / 180f;
                Vector2 footDir = new Vector2(MathF.Cos(legAngleRad), MathF.Sin(legAngleRad));
                Vector2 target = anchor + footDir * Config.FootRadialOffset;

                int segCount = Config.SegmentsPerLeg;
                var segments = new GameObject[segCount];

                for (int s = 0; s < segCount; s++)
                {
                    var segObj = new GameObject($"Leg{idx}_seg{s}");
                    GameObject.Scene?.AddGameObject(segObj);
                    var rend = segObj.AddComponent<SpriteRenderer>();
                    var src = s % 2 == 0 ? UpperSrc : LowerSrc;
                    rend.Sprite = new Sprite(Config.LegTex, src) { PixelsPerUnit = 32 };
                    rend.SortingOrder = -1;
                    segments[s] = segObj;
                    _legVisuals.Add(segObj);
                }

                float sinA = MathF.Sin(legAngleRad);
                float kneeDir;
                if (MathF.Abs(sinA) < 0.05f)
                    kneeDir = side == 0 ? 1f : -1f;
                else
                    kneeDir = (side == 0 ? 1f : -1f) * MathF.Sign(sinA);

                _legs[idx] = new LegData
                {
                    Group = idx % 2,
                    Side = side,
                    Idx = idx,
                    AnchorLocal = anchor,
                    TargetLocal = target,
                    FootWorld = Transform.WorldPosition + target,
                    SwingPhaseOffset = -idx * 0.05f,
                    Segments = segments,
                    KneeDir = kneeDir,
                    HasSwungThisCycle = false,
                };
            }
        }

        public override void OnDestroy()
        {
            foreach (var go in _legVisuals) go.Destroy();
            _legVisuals.Clear();
            if (_debugTargets != null)
                foreach (var go in _debugTargets) go?.Destroy();
            if (_debugFoots != null)
                foreach (var go in _debugFoots) go?.Destroy();
            _debugTex?.Dispose();
        }

        public override void Update(float dt)
        {
            var config = Config;
            if (config == null) return;

            Vector2 bodyPos = Transform.WorldPosition;
            Vector2 dir = _mover.GetDirection();
            float speed = dir.Length;

            if (speed > 0.01f)
            {
                float targetRot = MathF.Atan2(dir.Y, dir.X) - MathF.PI / 2;
                Transform.Rotation = RotationSlerp(Transform.Rotation, targetRot, dt * config.RotationSpeed);
            }

            float bodyRot = Transform.Rotation;
            float threshold = speed < 0.01f ? config.StepThreshold * 0.5f : config.StepThreshold;

            if (_activeGroup >= 0)
            {
                bool allLanded = true;
                for (int i = 0; i < config.LegCount; i++)
                    if (_legs[i].Group == _activeGroup && _legs[i].Swinging)
                    { allLanded = false; break; }
                if (allLanded) _activeGroup = -1;
            }

            if (_activeGroup == -1 && speed > 0.01f)
            {
                float groupError = 0f;
                int worstGroup = -1;

                for (int g = 0; g < 2; g++)
                {
                    float maxDist = 0f;
                    for (int i = 0; i < config.LegCount; i++)
                    {
                        if (_legs[i].Group != g) continue;
                        Vector2 wt = bodyPos + Rotate(_legs[i].TargetLocal, bodyRot);
                        float d = Vector2.Distance(_legs[i].FootWorld, wt);
                        if (d > maxDist) maxDist = d;
                    }
                    if (maxDist > groupError)
                    {
                        groupError = maxDist;
                        worstGroup = g;
                    }
                }

                if (groupError > threshold)
                {
                    _activeGroup = worstGroup;
                    for (int i = 0; i < config.LegCount; i++)
                        _legs[i].HasSwungThisCycle = false;
                }
            }

            for (int i = 0; i < config.LegCount; i++)
                UpdateLeg(i, dt, bodyPos, bodyRot);
        }

        private void UpdateLeg(int index, float dt, Vector2 bodyPos, float bodyRot)
        {
            var config = Config;
            var leg = _legs[index];

            Vector2 worldAnchor = bodyPos + Rotate(leg.AnchorLocal, bodyRot);
            Vector2 worldTarget = bodyPos + Rotate(leg.TargetLocal, bodyRot);

            float maxReach = config.TotalLegLength * config.MaxReachFactor;

            if (!leg.Swinging)
            {
                bool shouldSwing = leg.Group == _activeGroup && !leg.HasSwungThisCycle;

                if (shouldSwing)
                {
                    leg.Swinging = true;
                    leg.HasSwungThisCycle = true;
                    leg.SwingProgress = leg.SwingPhaseOffset;

                    Vector2 startPos = leg.FootWorld;
                    float startDist = Vector2.Distance(worldAnchor, startPos);
                    if (startDist > maxReach)
                        startPos = worldAnchor + (startPos - worldAnchor) / startDist * maxReach;

                    leg.SwingStart = startPos;
                    leg.SwingEnd = worldTarget;
                }
            }

            if (leg.Swinging)
            {
                leg.SwingEnd = worldTarget;
                leg.SwingProgress += dt / config.SwingDuration;

                if (leg.SwingProgress >= 1f)
                {
                    leg.FootWorld = leg.SwingEnd;
                    leg.Swinging = false;
                }
                else if (leg.SwingProgress > 0f)
                {
                    float t = leg.SwingProgress;
                    float eased = t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f;
                    float arc = MathF.Sin(eased * MathF.PI) * config.StepHeight;

                    Vector2 basePos = leg.SwingStart + (leg.SwingEnd - leg.SwingStart) * eased;
                    Vector2 up = new Vector2(-MathF.Sin(bodyRot), MathF.Cos(bodyRot));
                    Vector2 lifted = basePos + up * arc;

                    Vector2 toCenter = worldAnchor - lifted;
                    float centerDist = toCenter.Length;
                    if (centerDist > 0.001f) toCenter /= centerDist;
                    float foldFactor = MathF.Sin(eased * MathF.PI) * 0.4f;
                    leg.FootWorld = lifted + toCenter * foldFactor * centerDist;
                }
            }

            if (leg.Swinging)
            {
                float hipDist = Vector2.Distance(worldAnchor, leg.FootWorld);
                if (hipDist > maxReach)
                {
                    leg.FootWorld = worldAnchor + (leg.FootWorld - worldAnchor) / hipDist * maxReach;
                }
            }

            DrawLegIK(worldAnchor, leg.FootWorld, leg);
            _legs[index] = leg;

            if (Config.DebugMode && _debugTargets != null && _debugFoots != null)
            {
                _debugTargets[index].Transform.Position = worldTarget;
                _debugTargets[index].Transform.Scale = new Vector2(0.06f);
                _debugFoots[index].Transform.Position = leg.FootWorld;
                _debugFoots[index].Transform.Scale = new Vector2(0.06f);
            }
        }

        private void DrawLegIK(Vector2 hip, Vector2 foot, LegData leg)
        {
            int segCount = leg.Segments.Length;
            if (segCount < 1) return;

            var config = Config;
            float segLen = config.TotalLegLength / segCount;
            Vector2[] joints = SolveIK(hip, foot, segCount + 1, segLen, leg.KneeDir);

            for (int i = 0; i < segCount; i++)
            {
                Vector2 from = joints[i];
                Vector2 to = joints[i + 1];
                float srcH = i % 2 == 0 ? UpperSrc.Size.Y : LowerSrc.Size.Y;
                DrawSegment(leg.Segments[i], from, to, srcH);
            }
        }

        private Vector2[] SolveIK(Vector2 hip, Vector2 foot, int jointCount, float segLen, float kneeDir)
        {
            Vector2[] joints = new Vector2[jointCount];
            joints[0] = hip;
            joints[jointCount - 1] = foot;

            float totalLen = segLen * (jointCount - 1);
            float dist = Vector2.Distance(hip, foot);

            if (dist >= totalLen * 0.999f || dist < 0.001f)
            {
                for (int i = 1; i < jointCount - 1; i++)
                {
                    float t = (float)i / (jointCount - 1);
                    joints[i] = Vector2.Lerp(hip, foot, t);
                }
            }
            else if (jointCount == 3)
            {
                float dirAngle = MathF.Atan2(foot.Y - hip.Y, foot.X - hip.X);
                float hipAngle = MathF.Acos(MathF.Min(1f, dist / (2f * segLen)));

                joints[1] = new Vector2(
                    hip.X + segLen * MathF.Cos(dirAngle + hipAngle * kneeDir),
                    hip.Y + segLen * MathF.Sin(dirAngle + hipAngle * kneeDir));
            }
            else
            {
                Vector2 diff = foot - hip;
                float len = diff.Length;
                Vector2 dir = diff / len;
                Vector2 perp = new Vector2(-dir.Y, dir.X);

                float maxBend = MathF.PI * 0.5f;
                float bendAmt = (1f - dist / totalLen) * maxBend;

                for (int i = 1; i < jointCount - 1; i++)
                {
                    float t = (float)i / (jointCount - 1);
                    Vector2 basePos = Vector2.Lerp(hip, foot, t);
                    float bendFactor = MathF.Sin(t * MathF.PI) * bendAmt * kneeDir * segLen * 0.4f;
                    joints[i] = basePos + perp * bendFactor;
                }
            }

            return joints;
        }

        private void DrawSegment(GameObject obj, Vector2 from, Vector2 to, float srcHeight)
        {
            Vector2 diff = to - from;
            float dist = diff.Length;
            if (dist < 0.001f)
            {
                obj.Transform.Scale = Vector2.Zero;
                return;
            }

            obj.Transform.Position = (from + to) * 0.5f;
            obj.Transform.Rotation = MathF.Atan2(diff.Y, diff.X) + MathF.PI / 2;
            obj.Transform.Scale = new Vector2(1, dist * 32f / srcHeight);
        }

        private static float RotationSlerp(float from, float to, float t)
        {
            float diff = to - from;
            diff = (diff + MathF.PI) % (2f * MathF.PI) - MathF.PI;
            return from + diff * MathF.Min(t, 1f);
        }

        private static Vector2 Rotate(Vector2 v, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            return new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);
        }
    }
}
