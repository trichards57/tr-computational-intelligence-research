using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Media;

namespace CSharpSim.Classes
{
    class World
    {
        public List<Pod> Pods { get; set; }
        public List<Wall> Walls { get; set; }
        public Rect Rectangle { get; set; }
        public int Ticks { get; set; }
        public bool Blind { get; set; }

        public World(string filename, IEnumerable<Pod> pods)
        {
            Pods = new List<Pod>();
            Pods.AddRange(pods);
            Walls = new List<Wall>();

            using (var file = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                while (!file.EndOfStream)
                {
                    var l = file.ReadLine().Trim();
                    if (l.Length == 0 || l.StartsWith("#"))
                        continue;

                    var parts = l.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts[0] == "wall")
                    {
                        var wall = new Wall(parts[1]);
                        wall.Read(file);
                        Walls.Add(wall);
                        Rectangle = Rect.Union(Rectangle, wall.Rectangle);
                    }
                    else if (parts[0] == "pod")
                    {
                        ReadPodPosition(file);
                    }
                }
            }
        }

        private void ReadPodPosition(StreamReader file)
        {
            var line = file.ReadLine();
            var parts = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            var x = float.Parse(parts[0]);
            var y = float.Parse(parts[1]);

            foreach (var p in Pods)
            {
                p.X = x;
                p.Y = y;
            }
        }

        struct ClosestIntersect
        {
            public double tMin;
            public Wall wallMin;
        }

        ClosestIntersect FindClosestIntersect(Point p0, Point p1)
        {
            var tMin = 2.0;
            Wall wallMin = null; ;

            foreach (var w in Walls)
            {
                foreach (var s in w.Segments)
                {
                    var p2 = s.Point1;
                    var p3 = s.Point2;

                    var res = Interset(p0, p1, p2, p3);

                    if (res.t >= -float.Epsilon && res.t <= 1.0+float.Epsilon && res.s >= -float.Epsilon && res.s <= 1.0 float.Epsilon)
                    {
                        if (res.t < tMin)
                        {
                            tMin = res.t;
                            wallMin = w;
                        }
                    }
                }
            }

            return new ClosestIntersect { tMin = tMin, wallMin = wallMin };
        }

        struct IntersectResult
        {
            public double s;
            public double t;
        }

        IntersectResult Interset(Point p0, Point p1, Point p2, Point p3)
        {
            var line1 = p1 - p0;
            var s2 = p3 - p2;

            var fact = (-s2.X * line1.Y + line1.X * s2.Y);
    

            return new IntersectResult
            {
                s = (-line1.Y * (p0.X - p2.X) + line1.X * (p0.Y - p2.Y)) / fact,
                t = (-s2.X * (p0.Y - p2.Y) - s2.Y * (p0.X - p2.X)) / fact
            };
        }
    }
}
