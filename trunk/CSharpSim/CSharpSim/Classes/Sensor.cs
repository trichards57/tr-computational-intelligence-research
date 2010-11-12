using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CSharpSim.Classes
{
    class Sensor
    {
        public double ReferenceAngle { get; set; }
        public double Angle { get; set; }
        public double Range { get; set; }
        public double Value { get; set; }
        public string Wall { get; set; }
        public string Name { get; set; }

        public Sensor(double referenceAngle, double range, string name)
        {
            ReferenceAngle = referenceAngle;
            Angle = referenceAngle;
            Range = range;
            Value = 0;
            Wall = "None";
            Name = name;
        }

        public override string ToString()
        {
            return Range.ToString("f0", CultureInfo.CurrentCulture);
        }
    }
}
