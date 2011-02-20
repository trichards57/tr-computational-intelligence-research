namespace GeneticControllerOptimiser.Classes
{
    using global::System.Runtime.InteropServices;

    static class ThrusterStateHelper
    {
        public static void Limit(this ThrusterState state)
        {
            if (state.Up > 1)
                state.Up = 1;
            if (state.Up < 0)
                state.Up = 0;
            if (state.Left > 1)
                state.Left = 1;
            if (state.Left < 0)
                state.Left = 0;
            if (state.Right > 1)
                state.Right = 1;
            if (state.Right < 0)
                state.Right = 0;
        }
    }

    /// <summary>
    /// Represents the current state of the pod thrusters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct ThrusterState
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
    }
}
