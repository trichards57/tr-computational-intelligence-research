using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticControllerOptimiser.Classes
{
    class MultiRandom
    {
        private static readonly Random Global = new Random();

        [ThreadStatic]
        private static Random local;

        public static int Next()
        {
            var inst = local;
            if (inst == null)
            {
                int seed;
                lock (Global) seed = Global.Next();
                local = inst = new Random(seed);
            }
            return inst.Next();
        }

        public static int Next(int minValue, int maxValue)
        {
            var inst = local;
            if (inst == null)
            {
                int seed;
                lock (Global) seed = Global.Next();
                local = inst = new Random(seed);
            }
            return inst.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            var inst = local;
            if (inst == null)
            {
                int seed;
                lock (Global) seed = Global.Next();
                local = inst = new Random(seed);
            }
            return inst.NextDouble();
        }
    }
}
