using System;
using System.Collections.Generic;

namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Represents a genome for a controller.
    /// </summary>
    class Genome : List<double>, IEquatable<Genome>
    {
        /// <summary>
        /// Creates a new gene.
        /// </summary>
        /// <returns>A number between -50 and 50 that is used for a gene.</returns>
        /// <remarks>
        /// Used to create a consistent gene when generating a new genome and mutating
        /// an individual gene.
        /// 
        /// Uses the multi-threaded random class to try and increase the randomness of the genes.
        /// </remarks>
        public static double NewGene()
        {
            return MultiRandom.NextDouble() * 100 - 50;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Genome"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Genome"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Genome"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(Genome other)
        {
            if (other.Count != Count) return false;
            for (var i = 0; i < Count; i++)
                if (this[i] != other[i])
                    return false;
            return true;
        }
    }
}
