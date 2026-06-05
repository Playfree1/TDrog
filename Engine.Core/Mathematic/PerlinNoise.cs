

namespace Engine.Core.Mathematic
{
    public class PerlinNoise 
    {
        byte[] permutationTable = null!;

        public PerlinNoise( int seed = 0)
        {
            var rand = new Random(seed);
            permutationTable = new byte[1024];
            rand.NextBytes(permutationTable);
        }
        public float Noise(float fx, float fy, int octaves, float persistence)
        {
            float amplitude = 1;
            float max = 0;
            float result = 0;

            while (octaves-- > 0)
            {
                max += amplitude;
                result += Noise(fx, fy) * amplitude;
                amplitude *= persistence;
                fx *= 2;
                fy *= 2;
            }
            return result/max;
        }
        public float Noise(float fx, float fy)
        {
            int left = (int)Math.Floor(fx);
            int top = (int)Math.Floor(fy);

            float pointInQuadX = fx - left;
            float pointInQuadY = fy - top;

            float[] topLeftGradient = GetPseudoRandomGradient(left, top);
            float[] topRightGradient = GetPseudoRandomGradient(left + 1, top);
            float[] bottomLeftGradient = GetPseudoRandomGradient(left, top + 1);
            float[] bottomRightGradient = GetPseudoRandomGradient(left + 1, top + 1);

            float[] distanceToTopLeft = new float[] { pointInQuadX, pointInQuadY };
            float[] distanceToTopRight = new float[] { pointInQuadX - 1, pointInQuadY };
            float[] distanceToBottomLeft = new float[]{pointInQuadX, pointInQuadY - 1};
            float[] distanceToBottomRight = new float[]{pointInQuadX - 1, pointInQuadY - 1};

            float tx1 = Dot(distanceToTopLeft, topLeftGradient);
            float tx2 = Dot(distanceToTopRight, topRightGradient);
            float bx1 = Dot(distanceToBottomLeft, bottomLeftGradient);
            float bx2 = Dot(distanceToBottomRight, bottomRightGradient);

            pointInQuadX = QuanticCurve(pointInQuadX);
            pointInQuadY = QuanticCurve(pointInQuadY);

            float tx = Lerp(tx1, tx2, pointInQuadX);
            float bx = Lerp(bx1, bx2, pointInQuadX);
            float tb = Lerp(tx, bx, pointInQuadY);
            float normalized = (tb / 1.41421356237f)  + 0.5f;
            return Math.Clamp(normalized, 0f, 1f);
        }
        private float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }
        private float QuanticCurve(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        private float[] GetPseudoRandomGradient(int x, int y)
        {
            unchecked
            {
                int v = (int)(((x * 1836311903) ^ (y * 2971215073) + 4807526976) & 1023);
                v = permutationTable[v]&3;
                switch (v)            {
                    case 0: return new float[] { 0.7071f, 0.7071f };
                    case 1: return new float[] { -0.7071f, 0.7071f };
                    case 2: return new float[] { 0.7071f, -0.7071f };
                    default: return new float[] { -0.7071f, -0.7071f };
                }
            }
        }
        private float Dot(float[] a, float[] b)
        {
            return a[0] * b[0] + a[1] * b[1];
        }
    }
}