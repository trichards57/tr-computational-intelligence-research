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

        void FindClosestIntersect(Point p1, Point p2)
        {
            Vector t;
            Matrix m;
        }
    }
}
