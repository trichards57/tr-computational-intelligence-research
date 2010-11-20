using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace CSharpSim.Classes
{
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

        void Step(double dt)
        {
            Ticks++;

            foreach (var p in Pods)
            {
                p.Step(dt, this);
            }
        }
    }
}
