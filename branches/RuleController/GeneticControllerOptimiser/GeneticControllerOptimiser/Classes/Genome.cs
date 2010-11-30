using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticControllerOptimiser.Classes
{
    class Genome : List<double>, IEquatable<Genome>
    {
        private static ulong idCount = 0;
        private ulong ID = idCount++;

        public static double NewGene()
        {
            return MultiRandom.NextDouble() * 100 - 50;
        }

        public bool Equals(Genome other)
        {
            if (ID != other.ID)
                return false;
            if (other.Count != Count) return false;
            for (var i = 0; i < Count; i++)
                if (this[i] != other[i])
                    return false;
            return true;
        }
    }
}
