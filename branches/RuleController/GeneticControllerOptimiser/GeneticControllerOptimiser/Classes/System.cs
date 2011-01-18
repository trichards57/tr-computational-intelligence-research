using System;

namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Represents the system that the controller is controlling.
    /// </summary>
    class System
    {
        /// <summary>
        /// Gets or sets the current X coordinate.
        /// </summary>
        /// <value>The X coordinate.</value>
        private double X { get; set; }
        /// <summary>
        /// Gets or sets the current Y coordinate.
        /// </summary>
        /// <value>The Y coordinate.</value>
        private double Y { get; set; }
        /// <summary>
        /// Gets or sets the current angle.
        /// </summary>
        /// <value>The angle.</value>
        public double Angle { private get; set; }
        /// <summary>
        /// Gets or sets the horizontal velocity.
        /// </summary>
        /// <value>The horizontal velocity.</value>
        private double DxDt { get; set; }
        /// <summary>
        /// Gets or sets the vertical velocity.
        /// </summary>
        /// <value>The vertial velocity.</value>
        private double DyDt { get; set; }
        /// <summary>
        /// Gets or sets the angular velocity.
        /// </summary>
        /// <value>The angular velocity.</value>
        private double DAngleDt { get; set; }

        /// <summary>
        /// The constant acceleration due to gravity.
        /// </summary>
        private const double Gravity = 2.0;
        /// <summary>
        /// The mass of the pod.
        /// </summary>
        private const double Mass = 2.0;
        /// <summary>
        /// The moment of intertia of the pod.
        /// </summary>
        private const double Inertia = 0.5;
        /// <summary>
        /// The maximum thrust produced by the main thruster.
        /// </summary>
        private const double ThrustMax = 20;
        /// <summary>
        /// The maximum thrust produced by the lateral thrusters.
        /// </summary>
        private const double SpinThrustMax = 0.11;
        /// <summary>
        /// The simulation time step.
        /// </summary>
        private const double TimeStep = 0.1;

        /// <summary>
        /// Gets or sets a value indicating whether the simulation should disable gravity.
        /// </summary>
        /// <value><c>true</c> if the simulation should ignore gravity; otherwise, <c>false</c>.</value>
        public bool DisableGravity { private get; set; }

        /// <summary>
        /// Run a cycle of the simulator with the given thruster instructions.
        /// </summary>
        /// <param name="state">The state of the pod's thrusters.</param>
        /// <param name="target">The target state of the pod (used to detect if the pod has overshot).</param>
        /// <returns>The current state of the system.</returns>
        /// <remarks>This is designed to be the same as the simulation processing used in simulation.py</remarks>
        public SystemState Process(ThrusterState state, TargetState target)
        {
            state.Limit();

            X += DxDt * TimeStep;
            Y += DyDt * TimeStep;
            Angle += DAngleDt * TimeStep;

            var fail = false;

            if (Math.Abs(X) > target.XCutOff || Math.Abs(Y) > target.YCutOff || Math.Abs(Angle) > target.AngleCutOff)
            {
                fail = true;
            }


            DyDt = state.Up * ThrustMax * Math.Cos(Angle) / Mass;
            if (!DisableGravity)
                DyDt += Gravity;
            DxDt = state.Up * ThrustMax * Math.Sin(Angle) / Mass;
            DAngleDt = (state.Left - state.Right) * SpinThrustMax / Inertia;

            return new SystemState { Angle = Angle, DAngleDt = DAngleDt, DxDt = DxDt, DyDt = DyDt, X = X, Y = Y, OvershootFail = fail };
        }
    }
}
