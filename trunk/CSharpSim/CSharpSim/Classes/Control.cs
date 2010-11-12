using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSim.Classes
{
    class Control
    {
        public const double MaxValue = 1.0;
        public const double MinValue = 0.0;

        public double Left { get; set; }
        public double Right { get; set; }
        public double Up { get; set; }
        public double Down { get; set; }

        public void Limit()
        {
            Left = Utilities.Limit(Left, 0, 1);
            Right = Utilities.Limit(Right, 0, 1);
            Up = Utilities.Limit(Up, 0, 1);
            Down = Utilities.Limit(Down, 0, 1);
        }
    }
}
