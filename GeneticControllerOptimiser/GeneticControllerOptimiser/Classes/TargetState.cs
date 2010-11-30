using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticControllerOptimiser.Classes
{
    class TargetState
    {
        public double TargetX { get; set; }
        public double TargetY { get; set; }
        public double TargetAngle { get; set; }

        public double XCutOff { get; set; }
        public double YCutOff { get; set; }
        public double AngleCutOff { get; set; }

        public TargetState()
        {
            XCutOff = double.MaxValue;
            YCutOff = double.MaxValue;
            AngleCutOff = double.MaxValue;
        }
    }
}
