using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Core.Mathematic
{
    public class RandomF
    {
        private readonly Random _random = new Random();
        public float Range(float min, float max) => min + (float)(_random.NextDouble() * (max - min));
    }
}