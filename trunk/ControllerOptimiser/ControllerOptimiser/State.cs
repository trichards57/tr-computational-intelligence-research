using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControllerOptimiser
{
    using System.Windows;

    class State
    {
        public Point Location { get; set; }

        public Vector Velocity { get; set; }

        public double Angle { get; set; }

        public double DAngleDt { get; set; }

        public State(Pod pod)
        {
            Location = pod.Location;
            Velocity = pod.Velocity;
            Angle = pod.Angle;
            DAngleDt = pod.DAngleDt;

            TargetLocation = new Point(50, 50);
        }

        public Point TargetLocation { get; set; }
    }
}
