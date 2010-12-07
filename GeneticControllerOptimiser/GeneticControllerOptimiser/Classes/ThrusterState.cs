namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Represents the current state of the pod thrusters.
    /// </summary>
    class ThrusterState
    {
        /// <summary>
        /// Gets or sets the main thruster power.
        /// </summary>
        /// <value>The main thruster power, between 1.0 and 0.0, where 1.0 is maximum power.</value>
        public double Up { get; set; }
        /// <summary>
        /// Gets or sets the left thruster power.
        /// </summary>
        /// <value>The left thruster power, between 1.0 and 0.0, where 1.0 is maximum power.</value>
        public double Left { get; set; }
        /// <summary>
        /// Gets or sets the right thruster power.
        /// </summary>
        /// <value>The right thruster power, between 1.0 and 0.0, where 1.0 is maximum power.</value>
        public double Right { get; set; }

        /// <summary>
        /// Limits the thruster powers to be between 1 and 0.
        /// </summary>
        public void Limit()
        {
            if (Up > 1)
                Up = 1;
            if (Up < 0)
                Up = 0;
            if (Left > 1)
                Left = 1;
            if (Left < 0)
                Left = 0;
            if (Right > 1)
                Right = 1;
            if (Right < 0)
                Right = 0;
        }
    }
}
