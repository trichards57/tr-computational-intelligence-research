namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Represents the results achieved by a specific genome.
    /// </summary>
    /// <remarks>
    /// Used for system processing caching.
    /// </remarks>
    class SystemResults
    {
        /// <summary>
        /// Gets or sets the fitness of the system when moving in the positive direction.
        /// </summary>
        /// <value>The fitness.</value>
        public int Fitness { get; set; }
        /// <summary>
        /// Gets or sets the fitness of the system when moving in the negative direction.
        /// </summary>
        /// <value>The fitness.</value>
        public int NegativeFitness { get; set; }
    }
}
