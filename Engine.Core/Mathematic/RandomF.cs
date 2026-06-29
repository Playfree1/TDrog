using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Core.Mathematic
{
    public class RandomF
    {
        private static readonly Random _random = new Random();
        public static float Range(float min, float max) => min + (float)(_random.NextDouble() * (max - min));
    }
}