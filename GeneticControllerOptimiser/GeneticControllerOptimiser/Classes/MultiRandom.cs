using System;

namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// A random number generator redesigned to work with parallel functions.
    /// </summary>
    /// <remarks>
    /// Creates one random number generator <see cref="local"/> per thread, which is 
    /// seeded from a global random number generator <see cref="Global"/>.
    /// 
    /// Based on code from http://blogs.msdn.com/b/pfxteam/archive/2009/02/19/9434171.aspx.
    /// </remarks>
    static class MultiRandom
    {
        /// <summary>
        /// Global random number generator used to seed the thread local generators.
        /// </summary>
        private static readonly Random Global = new Random();

        /// <summary>
        /// Local random number generator. 
        /// </summary>
        /// <remarks>
        /// Marked ThreadStatic, so a different object exists on each thread, 
        /// so it doesn't need to be locked when accessing.
        /// </remarks>
        [ThreadStatic]
        private static Random local;

        /// <summary>
        /// Returns a random number within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">
        /// The exclusive upper bound of the random number returned. maxValue must be greater 
        /// than or equal to minValue.
        /// </param>
        /// <returns>
        /// A 32-bit signed integer greater than or equal to minValue and less than maxValue; 
        /// that is, the range of return values includes minValue but not maxValue. If 
        /// minValue equals maxValue, minValue is returned.
        /// </returns>
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

        /// <summary>
        /// Returns a random number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
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
