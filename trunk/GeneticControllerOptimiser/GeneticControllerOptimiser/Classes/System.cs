using System;

namespace GeneticControllerOptimiser.Classes
{
    class System
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public double DxDt { get; set; }
        public double DyDt { get; set; }
        public double DAngleDt { get; set; }

        public const double Gravity = 2.0;
        public const double Mass = 2.0;
        public const double Inertia = 0.5;
        public const double ThrustMax = 20;
        public const double SpinThrustMax = 0.11;
        public const double TimeStep = 0.1;

        public bool DisableGravity { get; set; }

        public SystemState Process(ThrusterState state)
        {
            state.Limit();

            X += DxDt * TimeStep;
            Y += DyDt * TimeStep;
            Angle += DAngleDt * TimeStep;

            DyDt = state.Up * ThrustMax * Math.Cos(Angle) / Mass;
            if (!DisableGravity)
                DyDt += Gravity;
            DxDt = state.Up * ThrustMax * Math.Sin(Angle) / Mass;
            DAngleDt = (state.Left - state.Right) * SpinThrustMax / Inertia;

            return new SystemState { Angle = Angle, DAngleDt = DAngleDt, DxDt = DxDt, DyDt = DyDt, X = X, Y = Y };
        }
    }
}
