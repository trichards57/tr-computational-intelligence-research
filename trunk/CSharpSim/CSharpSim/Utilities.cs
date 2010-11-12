using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace CSharpSim
{
    static class Utilities
    {
        public static double Limit(double value, double max, double min)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        internal static List<Point> ReadPoints(StreamReader data)
        {
            throw new NotImplementedException();
        }
    }
}
