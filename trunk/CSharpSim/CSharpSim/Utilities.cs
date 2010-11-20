using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace CSharpSim
{
    using System.Windows.Media;

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

        public static Point Intersect(Point p0, Point p1, Point p2, Point p3)
        {
            var line1 = new LineGeometry(p0, p1).GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));
            var line2 = new LineGeometry(p2, p3).GetWidenedPathGeometry(new Pen(Brushes.Black, 1.0));

            var result = new CombinedGeometry(GeometryCombineMode.Intersect, line1, line2);
            var flatGeo = result.GetFlattenedPathGeometry();

            if (flatGeo.Figures.Count > 0)
            {
                var fig = new PathGeometry(new[] { flatGeo.Figures[0] }).Bounds;
                var res = new Point(fig.Left + fig.Width / 2, fig.Top + fig.Height / 2);
                return res;
            }

            //var line1 = p1 - p0;
            //var line2 = p3 - p2;

            //var ua = (line2.X * line1.X - line2.Y * line1.Y) / (line2.Y * line1.X - line2.X * line1.Y);

            //var res = p0 + ua * line1;
            

            //if (res.X >= Math.Min(p1.X, p2.X) && res.X <= Math.Max(p1.X, p2.X))
            //    return res;
            return new Point(double.PositiveInfinity, double.PositiveInfinity);
        }
    }
}
