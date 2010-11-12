using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSim.Classes
{
    class Pod
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double DxDt { get; set; }

        public double DyDt { get; set; }

        public double Angle { get; set; }

        public double DAngleDt { get; set; }
    }
}
