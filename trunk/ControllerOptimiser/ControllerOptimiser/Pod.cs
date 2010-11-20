using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControllerOptimiser
{
    using System.Windows;

    class Pod
    {
        public const double Gravity = 2;

        public const double Mass = 2;

        public const double Inertia = 0.5;

        public const double ThrustMax = 20;

        public const double SpinThrustMax = 0.11;

        public Brain Brain { get; set; }

        public double Angle { get; set; }

        public double DAngleDt { get; set; }

        public Point Location { get; set; }

        public Vector Velocity { get; set; }

        public Control Control { get; set; }

        public Pod(Brain brain)
        {
            Angle = Math.PI;
            Brain = brain;
        }

        public void Step(double dt)
        {
            var state = new State(this);
            Control = Brain.Process(state, dt);
            Control.Limit();

            Location += Velocity * dt;
            var thrust = ThrustMax * Control.Up;

            Velocity += new Vector(thrust * Math.Sin(Angle) / Mass,thrust * Math.Cos(Angle) / Mass + Gravity);
            Angle += DAngleDt * dt;
            DAngleDt += (-Control.Right + Control.Left) * SpinThrustMax / Inertia;
        }
    }
}
