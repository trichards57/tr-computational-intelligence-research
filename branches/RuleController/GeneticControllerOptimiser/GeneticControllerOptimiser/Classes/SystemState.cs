namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Represents the current state of the pod.
    /// </summary>
    class SystemState
    {
        /// <summary>
        /// Gets or sets the current X coordinate.
        /// </summary>
        /// <value>The X coordinate.</value>
        public double X { get; set; }
        /// <summary>
        /// Gets or sets the current Y coordinate.
        /// </summary>
        /// <value>The Y coordinate.</value>
        public double Y { get; set; }
        /// <summary>
        /// Gets or sets the current horizontal velocity.
        /// </summary>
        /// <value>The horizontal velocity.</value>
        public double DxDt { get; set; }
        /// <summary>
        /// Gets or sets the current vertical velocity.
        /// </summary>
        /// <value>The vertical velocity.</value>
        public double DyDt { get; set; }
        /// <summary>
        /// Gets or sets the current angle.
        /// </summary>
        /// <value>The angle.</value>
        public double Angle { get; set; }
        /// <summary>
        /// Gets or sets the current angular velocity.
        /// </summary>
        /// <value>The angular velocity.</value>
        public double DAngleDt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the system has failed by overshooting..
        /// </summary>
        /// <value><c>true</c> if the system overshot; otherwise, <c>false</c>.</value>
        public bool OvershootFail { get; set; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("X {0} Y {1} A {2}", X, Y, Angle);
        }
    }
}
