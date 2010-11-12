using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSim.Classes
{
    class State
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double DxDt { get; set; }
        public double DyDt { get; set; }
        public double Angle { get; set; }
        public double DAngleDt { get; set; }

        public State(Pod pod)
        {
            X = pod.X;
            Y = pod.Y;
            DxDt = pod.DxDt;
            DyDt = pod.DyDt;
            Angle = pod.Angle;
            DAngleDt = pod.DAngleDt;
        }
    }
}
