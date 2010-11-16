using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace CSharpSim
{
    struct IntersectResult
    {
        public double s;
        public double t;
    }

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
            var points = new List<Point>();

            while (true)
            {
                var l = data.ReadLine();
                
                if (string.IsNullOrWhiteSpace(l))
                    return points;
                if (l.StartsWith("#"))
                    continue;

                var parts = l.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i + 1 < parts.Length; i+=2)
                {
                    var x = double.Parse(parts[i]);
                    var y = double.Parse(parts[i + 1]);

                    points.Add(new Point(x, y));
                }
            }
        }

        public static IntersectResult Intersect(Point p0, Point p1, Point p2, Point p3)
        {
            var line1 = p1 - p0;
            var line2 = p3 - p2;

            var fact = (-line2.X * line1.Y + line1.X * line2.Y);


            return new IntersectResult
            {
                s = (-line1.Y * (p0.X - p2.X) + line1.X * (p0.Y - p2.Y)) / fact,
                t = (-line2.X * (p0.Y - p2.Y) - line2.Y * (p0.X - p2.X)) / fact
            };
        }
    }
}
