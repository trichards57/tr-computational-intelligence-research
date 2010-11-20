using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CSharpSim
{
    using System.Threading;
    using System.Windows.Threading;

    using CSharpSim.Classes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Timer timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdatePod(object state)
        {
            var world = state as World;
            if (world == null)
                return;

            foreach (var p in world.Pods)
            {
                p.Location = new Point(p.X + 10, p.Y - 10);
            }

            Dispatcher.Invoke(new Action(UpdateCanvas), null);
        }

        private void UpdateCanvas()
        {
            foreach (var c in MainCanvas.Children)
            {
                var path = c as Path;
                if (path != null)
                    UpdatePodSensorsPath(path);

                var poly = c as Polygon;
                if (poly != null)
                    UpdatePodPolygon(poly);
            }
        }

        private World World { get; set; }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var pods = new[] { new Pod(40, 2000, null, Colors.Red) };
            World = new World(@"C:\Users\Tony\Documents\Computational Intelligance Program\world.txt", pods);

            MainCanvas.Width = World.Rectangle.Width;
            MainCanvas.Height = World.Rectangle.Height;
            #region Draw Walls
            foreach (var wall in World.Walls)
            {
                var color = Colors.Blue;
                if (wall.Name == "end")
                    color = Colors.Cyan;

                var segments = wall.Segments.Select(s => new LineSegment(s.Point2, true));
                var f = new PathFigure
                {
                    StartPoint = wall.Segments.First().Point1,
                    Segments = new PathSegmentCollection(segments)
                };

                f.Segments.Add(new LineSegment(wall.Segments.Last().Point1, true));

                var geometry = new PathGeometry();
                geometry.Figures.Add(f);
                var path = new Path
                {
                    Data = geometry,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 6
                };

                MainCanvas.Children.Add(path);
            }
            #endregion

            foreach (var path in pods.Select(pod => new Path
            {
                Tag = pod,
                Stroke = new SolidColorBrush(Colors.Gray)
            }))
            {
                UpdatePodSensorsPath(path);

                MainCanvas.Children.Add(path);
            }

            foreach (var podPoly in pods.Select(p => new Polygon
            {
                Tag = p,
                Fill = new SolidColorBrush(p.Color)
            }))
            {
                UpdatePodPolygon(podPoly);

                MainCanvas.Children.Add(podPoly);
            }

            timer = new Timer(UpdatePod, World, 0, 1000);
        }

        private void UpdatePodSensorsPath(Path path)
        {
            var pod = path.Tag as Pod;

            if (pod == null)
                return;

            var segments = new List<LineSegment>();

            foreach (var endPoint in from s in pod.Sensors select new Vector(s.Range * Math.Cos(s.Angle), s.Range * Math.Sin(s.Angle)) into sensorVector select pod.Location + sensorVector into endSensorPoint select NearestWallIntersect(pod.Location, endSensorPoint))
            {
                segments.Add(new LineSegment(endPoint, true));
                segments.Add(new LineSegment(pod.Location, false));
            }

            var figure = new PathFigure(pod.Location, segments, true);

            var geo = new PathGeometry(new[] { figure });
            path.Data = geo;
        }

        private Point NearestWallIntersect(Point startSensorPoint, Point endSensorPoint)
        {
            var intersectPoint = endSensorPoint;
            var lastDifference = double.MaxValue;

            foreach (var w in World.Walls)
            {
                foreach (var s in w.Segments)
                {
                    var intersect = Utilities.Intersect(startSensorPoint, endSensorPoint, s.Point1, s.Point2);
                    if (double.IsInfinity(intersect.X) || double.IsInfinity(intersect.Y))
                        continue;
                    var sLine = startSensorPoint - intersect;
                    if (Math.Abs(sLine.Length) >= lastDifference) continue;
                    lastDifference = Math.Abs(sLine.Length);
                    intersectPoint = intersect;
                }
            }

            return intersectPoint;
        }

        private static void UpdatePodPolygon(Polygon polygon)
        {
            var pod = polygon.Tag as Pod;

            if (pod == null)
                return;

            if (polygon.Points.Count != 4)
            {
                for (var i = 0; i < 4; i++)
                    polygon.Points.Add(new Point());
            }

            var podPos = new Vector(pod.X, pod.Y);

            polygon.Points[0] = new Point(-10, 10) + podPos;
            polygon.Points[1] = new Point(0, -20) + podPos;
            polygon.Points[2] = new Point(10, 10) + podPos;
            polygon.Points[3] = new Point(-10, 10) + podPos;
        }
    }
}
