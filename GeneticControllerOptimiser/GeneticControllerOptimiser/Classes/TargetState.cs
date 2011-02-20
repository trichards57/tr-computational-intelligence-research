namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Represents the target state of the system
    /// </summary>
    /// <remarks>Used for overshoot detection</remarks>
    class TargetState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        /// <summary>
        /// Gets or sets the X coordinate cut off.
        /// </summary>
        /// <value>The X coordinate cut off.</value>
        public double XCutOff { get; set; }
        /// <summary>
        /// Gets or sets the Y coordinate cut off.
        /// </summary>
        /// <value>The Y coordinate cut off.</value>
        public double YCutOff { get; set; }
        /// <summary>
        /// Gets or sets the angle cut off.
        /// </summary>
        /// <value>The angle cut off.</value>
        public double AngleCutOff { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetState"/> class.
        /// </summary>
        /// <remarks>
        /// Sets the cut off values to the maximum possible value for a double, 
        /// effectivly disabling the cut off detection (it's impossible to be greater).
        /// </remarks>
        public TargetState()
        {
            XCutOff = double.MaxValue;
            YCutOff = double.MaxValue;
            AngleCutOff = double.MaxValue;
        }
    }
}
