using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace CSharpSim.Classes
{
    using System.Linq;

    class World
    {
        public List<Pod> Pods { get; set; }
        public List<Wall> Walls { get; set; }
        public Rect Rectangle { get; set; }
        public int Ticks { get; set; }
        public bool Blind { get; set; }

        public World()
        {
            Pods = new List<Pod>();
            Walls = new List<Wall>();
        }

        public World(string filename, IEnumerable<Pod> pods) : this()
        {
            if (pods != null)
                Pods.AddRange(pods);
            
            using (var file = new StreamReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                while (!file.EndOfStream)
                {
                    var l = file.ReadLine();
                    if (l == null) continue;
                    l = l.Trim();
                    if (l.Length == 0 || l.StartsWith("#"))
                        continue;

                    var parts = l.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    switch (parts[0])
                    {
                        case "wall":
                            var wall = new Wall(parts[1]);
                            wall.Read(file);
                            Walls.Add(wall);
                            Rectangle = Rect.Union(Rectangle, wall.Rectangle);
                            break;
                        case "pod":
                            ReadPodPosition(file);
                            break;
                    }
                }
            }
        }

        private void ReadPodPosition(TextReader file)
        {
            var line = file.ReadLine();
            if (line == null) return;
            var parts = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            var x = float.Parse(parts[0]);
            var y = float.Parse(parts[1]);

            foreach (var p in Pods)
            {
                p.X = x;
                p.Y = y;
            }
        }

        internal struct ClosestIntersect
        {
            public double TMin;
            public Wall WallMin;
        }

        public ClosestIntersect FindClosestIntersect(Point p0, Point p1)
        {
            var tMin = 2.0;
            Wall wallMin = null;

            foreach (var w in Walls)
            {
                var min = tMin;
                foreach (var res in from s in w.Segments let p2 = s.Point1 let p3 = s.Point2 select Utilities.Intersect(p0, p1, p2, p3) into res where res.t >= 0 && res.t <= 1.0 && res.s >= 0 && res.s <= 1.0 where res.t < min select res)
                {
                    tMin = res.t;
                    wallMin = w;
                }
            }

            return new ClosestIntersect { TMin = tMin, WallMin = wallMin };
        }

        Wall CheckCollideWithWall(Point p0, Point p1)
        {
            return p0 == p1 ? null : Walls.FirstOrDefault(w => w.Segments.Select(s => Utilities.Intersect(p0, p1, s.Point1, s.Point2)).Any(res => res.s >= 0 && res.s <= 1 && res.t >= 0 && res.t <= 1));
        }

        void Step(double dt)
        {
            Ticks++;

            foreach (var p in Pods)
            {
                p.Step(dt, this);
                p.UpdateSensors(this);
            }
        }
    }
}
