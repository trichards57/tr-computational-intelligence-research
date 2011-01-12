using System;

/// @package GeneticControllerOptimiser.Classes
/// @brief Contains all the classes used by the genetic controller optimiser.

namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// The controller used to test each genome.
    /// </summary>
    class Controller
    {
        /// <summary>
        /// Gets or sets the largest Y speed threshold.
        /// </summary>
        /// <value>The largest Y speed threshold.</value>
        public double BigYSpeed { get; set; }
        /// <summary>
        /// Gets or sets the middle Y speed threshold.
        /// </summary>
        /// <value>The middle Y speed threshold.</value>
        public double MidYSpeed { get; set; }
        /// <summary>
        /// Gets or sets the smallest Y speed threshold.
        /// </summary>
        /// <value>The small Y speed threshold.</value>
        public double SmlYSpeed { get; set; }
        /// <summary>
        /// Gets or sets the largest X speed threshold.
        /// </summary>
        /// <value>The largest X speed threshold.</value>
        public double BigXSpeed { get; set; }
        /// <summary>
        /// Gets or sets the middle X speed threshold.
        /// </summary>
        /// <value>The middle X speed threshold.</value>
        public double MidXSpeed { get; set; }
        /// <summary>
        /// Gets or sets the smallest X speed threshold.
        /// </summary>
        /// <value>The small X speed threshold.</value>
        public double SmlXSpeed { get; set; }

        /// <summary>
        /// Gets or sets the largest X error threshold.
        /// </summary>
        /// <value>The largest X error threshold.</value>
        public double BigXError { get; set; }
        /// <summary>
        /// Gets or sets the middle X error threshold.
        /// </summary>
        /// <value>The middle X error threshold.</value>
        public double MidXError { get; set; }

        /// <summary>
        /// Gets or sets the largest Y error threshold.
        /// </summary>
        /// <value>The largest Y error threshold.</value>
        public double BigYError { get; set; }
        /// <summary>
        /// Gets or sets the middle Y error threshold.
        /// </summary>
        /// <value>The middle Y error treshold.</value>
        public double MidYError { get; set; }

        /// <summary>
        /// Gets or sets the force used to accelerate the pod up.
        /// </summary>
        /// <value>Upward propulsion force.</value>
        public double UpForce { get; set; }
        /// <summary>
        /// Gets or sets the force used to accelerate the pod down.
        /// </summary>
        /// <value>Downward propulsion force.</value>
        public double DownForce { get; set; }

        /// <summary>
        /// Gets or sets the angle used to accelerate the pod sidewise.
        /// </summary>
        /// <value>The horizontal movement angle.</value>
        public double PropelAngle { get; set; }

        /// <summary>
        /// Gets or sets the proportional gain for the angle controller.
        /// </summary>
        /// <value>The angle proportional gain.</value>
        public double AngleProportionalGain { get; set; }
        /// <summary>
        /// Gets or sets the differential gain for the angle controller.
        /// </summary>
        /// <value>The angle differential gain.</value>
        public double AngleDifferentialGain { get; set; }
        /// <summary>
        /// Gets or sets the integral gain for the angle controller.
        /// </summary>
        /// <value>The angle integral gain.</value>
        public double AngleIntegralGain { get; set; }

        /// <summary>
        /// Gets or sets the last side force.
        /// </summary>
        /// <value>The last side force.</value>
        public double LastSideForce { get; set; }
        /// <summary>
        /// Gets or sets the angle error integral.
        /// </summary>
        /// <value>The angle error integral.</value>
        /// <remarks>Used by the angle control</remarks>
        public double AngleErrorIntegral { get; set; }

        /// <summary>
        /// Creates a controller from the given <paramref name="genome"/>.
        /// </summary>
        /// <param name="genome">The genome to create a controller from.</param>
        /// <returns>
        /// A new controller with parameters determined by the <paramref name="genome"/>.
        /// 
        /// The genome is mapped such that the X properties are first, then the Y properties,
        /// then the angle controls.  This simplifies selective mutation and breeding.
        /// 
        /// At present, <see cref="PropelAngle"/>, <see cref="UpForce"/> and <see cref="DownForce"/>
        /// are fixed and the associated genes are ignored.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the genome provided is not 16 genes long.</exception>
        public static Controller FromGenome(Genome genome)
        {
            if (genome.Count != 16)
                throw new ArgumentException("Genome is not the right length.", "genome");
            return new Controller
            {

                BigXSpeed = genome[0] + 50, // Must be positive
                MidXSpeed = genome[1] + 50,  // Must be positive
                SmlXSpeed = genome[2] + 50, // Must be positive
                BigXError = genome[3] + 50, // Must be positive
                MidXError = genome[4] + 50, // Must be positive
                PropelAngle = 0.1, // Must be positive and between 2 and -2
                BigYSpeed = genome[6] + 50, // Must be positive
                MidYSpeed = genome[7] + 50, // Must be positive
                SmlYSpeed = genome[8] + 50, // Must be positive
                BigYError = genome[9] + 50, // Must be positive
                MidYError = genome[10] + 50, // Must be positive
                UpForce = 0.4, // Set to save time
                DownForce = 0.0, // Set to save time
                AngleProportionalGain = genome[13],
                AngleDifferentialGain = genome[14],
                AngleIntegralGain = genome[15]
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        private Controller()
        {
        }

        /// <summary>
        /// Runs the controller with the given state.
        /// </summary>
        /// <param name="state">The state of the system.</param>
        /// <param name="targetX">The target X coordinate.</param>
        /// <param name="targetY">The target Y coordinate.</param>
        /// <param name="targetAngle">The target angle. Should be set to the angle that points the pod straight up in environment units (not normalised units).</param>
        /// <returns>A set of thruster instructions to control the system.</returns>
        /// <remarks>
        /// The controller operates in the same way as the <see cref="Controllers.RuleController"/>.
        /// </remarks>
        public ThrusterState Process(SystemState state, double targetX, double targetY, double targetAngle)
        {
            var control = new ThrusterState();
            var xError = targetX - state.X;
            var yError = targetY - state.Y;

            var normalisedAngle = (2 * Math.PI) - ((state.Angle + Math.PI) % (2 * Math.PI));
            if (normalisedAngle > Math.PI)
                normalisedAngle -= 2 * Math.PI;

            if (yError < 0)
                control.Up = 0.5;

            double maxSpeed;

            if (Math.Abs(yError) > BigYError)
                maxSpeed = BigYSpeed;
            else if (Math.Abs(yError) > MidYError)
                maxSpeed = MidYSpeed;
            else
                maxSpeed = SmlYSpeed;

            if (state.DyDt < -maxSpeed)
                control.Up = DownForce;
            else if (state.DyDt > maxSpeed)
                control.Up = UpForce;

            if (xError > 1)
                targetAngle = PropelAngle;
            else if (xError < -1)
                targetAngle = -PropelAngle;

            if (Math.Abs(xError) > BigXError)
                maxSpeed = BigXSpeed;
            else if (Math.Abs(xError) > MidXError)
                maxSpeed = MidXSpeed;
            else
                maxSpeed = SmlXSpeed;

            if (state.DxDt > maxSpeed)
                targetAngle = -PropelAngle;
            else if (state.DxDt < -maxSpeed)
                targetAngle = PropelAngle;

            var angError = targetAngle - normalisedAngle;

            AngleErrorIntegral += angError;

            var sideForce = (angError * AngleProportionalGain + state.DAngleDt * AngleDifferentialGain/* + AngleErrorIntegral * AngleIntegralGain*/);

            if (sideForce > 0)
            {
                control.Right = sideForce;
                control.Left = 0;
            }
            else
            {
                control.Left = -sideForce;
                control.Right = 0;
            }

            LastSideForce = sideForce;

            return control;
        }
    }
}
