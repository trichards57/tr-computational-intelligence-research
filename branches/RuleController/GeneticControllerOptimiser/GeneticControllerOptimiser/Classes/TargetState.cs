using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Represents the target state of the system
    /// </summary>
    /// <remarks>Used for overshoot detection</remarks>
    class TargetState
    {
        /// <summary>
        /// Gets or sets the target X coordinate.
        /// </summary>
        /// <value>The target X coordinate.</value>
        public double TargetX { get; set; }
        /// <summary>
        /// Gets or sets the target Y coordinate.
        /// </summary>
        /// <value>The target Y coordinate.</value>
        public double TargetY { get; set; }
        /// <summary>
        /// Gets or sets the target angle.
        /// </summary>
        /// <value>The target angle.</value>
        public double TargetAngle { get; set; }

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
