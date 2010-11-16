using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSim.Classes
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    class Pod
    {
        public static Polygon PodPoly = new Polygon {
            Points = new PointCollection(new[] { new Point(-10, -10), new Point(0, 20), new Point(10, -10) })
        };

        public static Polygon ThrustPoly = new Polygon {
            Points = new PointCollection(new[] { new Point(0, -10), new Point(-2, -14), new Point(0, -18), new Point(2, -14) })
        };

        public static Polygon LeftPoly = new Polygon {
            Points = new PointCollection(new[] { new Point(-5, 5), new Point(-9, 4), new Point(-12, 5), new Point(-9, 6) })
        };

        public static Polygon RightPoly = new Polygon
        {
            Points = new PointCollection(new[] { new Point(5, 5), new Point(9, 4), new Point(12, 5), new Point(9, 6) })
        };

        public Pod(int nSensor, double sensorRange, Brain brain, Color color)
        {
            Color = color;
            Angle = Math.PI;
            Brain = Brain;

            Sensors = new List<Sensor>();
            Control = new Control();

            for (var i = 0; i < nSensor; i++)
            {
                var angRef = i * Math.PI * 2 / nSensor;
                Sensors.Add(new Sensor(angRef, sensorRange, string.Format("sensor{0}", i)));
            }
        }

        public Brain Brain { get; set; }

        public Color Color { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double DxDt { get; set; }

        public double DyDt { get; set; }

        public double Velocity { get; set; }

        public double Angle { get; set; }

        public double DAngleDt { get; set; }

        public bool Collide { get; set; }

        public int CollideCount { get; set; }

        public List<Sensor> Sensors { get; set; }

        public Control Control { get; set; }

        internal void Step(double dt, World world)
        {
            throw new NotImplementedException();
        }

        internal void UpdateSensors(World world)
        {
            foreach (var s in Sensors)
            {
                s.Angle = s.ReferenceAngle + Angle;
                var res = world.FindClosestIntersect(new Point(X, Y), new Point(X + s.Range * Math.Sin(Angle), Y + s.Range * Math.Cos(Angle)));
                s.Value = res.TMin * s.Range;
                if (res.WallMin == null)
                    s.Wall = null;
                else
                    s.Wall = res.WallMin.Name;
            }
        }

        internal void Draw(System.Windows.Controls.Canvas canvas)
        {
            DrawSensors(canvas);
            DrawPod(canvas);
        }

        private void DrawPod(System.Windows.Controls.Canvas canvas)
        {
            if (Collide)
                CollideCount = 100;
        }

        private void DrawSensors(System.Windows.Controls.Canvas canvas)
        {
            throw new NotImplementedException();
        }
    }
}
