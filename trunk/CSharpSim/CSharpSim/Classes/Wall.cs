using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace CSharpSim.Classes
{
    class WallSegment
    {
        public Point Point1 { get; set; }
        public Point Point2 { get; set; }
    }

    class Wall
    {
        public string Name { get; set; }
        public List<WallSegment> Segments { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double MinX { get; set; }
        public double MinY { get; set; }
        public Rect Rectangle { get; set; }

        public Wall(string name)
        {
            Segments = new List<WallSegment>();
            Name = name;
        }
        
        public void Read(StreamReader data)
        {
            var points = Utilities.ReadPoints(data);

            for (var i = 0; i + 1 < points.Count; i += 1)
            {
                Segments.Add(new WallSegment { Point1 = points[i], Point2 = points[i + 1] });
            }

            MaxX = points.AsParallel().Max(p => p.X);
            MinX = points.AsParallel().Min(p => p.X);
            MaxY = points.AsParallel().Max(p => p.Y);
            MinY = points.AsParallel().Min(p => p.Y);

            Rectangle = new Rect(MinX, MinY, MaxX - MinX, MaxY - MinY);
        }
    }
}
