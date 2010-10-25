using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MultiAgentLab.Classes
{
    using System.Diagnostics;

    class Field : ObservableCollection<FieldRow>
    {
        public Point StartPoint;

        public Field(int width) : this(width, width) { }

        public Field(int width, int height)
        {
            for (var i = 0; i < height; i++)
            {
                Add(new FieldRow(i, width));
            }
        }

        public Field(int width, int height, string filename)
            : this(width, height)
        {
            // File Format : 
            //
            // x,y,sensor0ang,sensor0val,sensor0state,sensor1ang,sensor1val,sensor1state,...,sensor39ang,sensor39val,sensor39state
            // x,y,sensor0ang,sensor0val,sensor0state,sensor1ang,sensor1val,sensor1state,...,sensor39ang,sensor39val,sensor39state
            //
            // Repeat as required...
            //
            // x, y and val values are floats.
            // state values should be strings.
            //
            // sensornval is a floating point number.

            // Read the file in to memory.
            // Convert to a list to make sure all the reading is done now, not on demand.
            var lines = File.ReadLines(filename).ToList();

            // Work out the starting point.
            var firstLineParts = lines.First().Split(new[] { "," }, 3, StringSplitOptions.RemoveEmptyEntries);
            StartPoint = new Point(double.Parse(firstLineParts[0]), double.Parse(firstLineParts[1]));

            // Split each line up in to a list of readings, then merge them all together.
            // Don't care about order, so can be done in parallel.
            var readings = lines.AsParallel().SelectMany(l =>
                {
                    var parts = l.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var origin = new Point(double.Parse(parts[0]), double.Parse(parts[1]));

                    var output = new List<SensorReading>();
                    for (var i = 2; i < parts.Length; i += 3)
                    {
                        var reading = new SensorReading
                        {
                            Angle = double.Parse(parts[i]),
                            Range = double.Parse(parts[i + 1]),
                            State = (SensorState)Enum.Parse(typeof(SensorState), parts[i + 2], true),
                            Origin = origin
                        };

                        output.Add(reading);
                    }
                    return output;
                });

            // Process each reading in to a coordinate and a state. Again, order is irrelevant, so can be done in
            // in parallel.
            var rawPoints = readings.Select(r => new
            {
                Point = new Point(r.Origin.X + r.Range * Math.Sin(r.Angle), r.Origin.Y + r.Range * Math.Cos(r.Angle)),
                r.State
            });

            // Work out the dimensions of the map
            var maxX = rawPoints.Max(p => p.Point.X);
            var maxY = rawPoints.Max(p => p.Point.Y);

            // Get the size of each new map square
            var yRectSize = maxY / (Count - 1);
            var xRectSize = maxX / (this.First().Count - 1);

            // Scale the starting point
            StartPoint.X = (int)Math.Round(StartPoint.X / xRectSize);
            StartPoint.Y = (int)Math.Round(StartPoint.Y / yRectSize);

            // Process each sensor point and transfer to the new map.
            // Can't do this parallel.  Too much chance of race conditions.
            foreach (var p in rawPoints.ToList())
            {
                var newPoint = new Point(Math.Round(p.Point.X / xRectSize), Math.Round(p.Point.Y / yRectSize));

                if (p.State == SensorState.End)
                {
                    this[(int)newPoint.Y][(int)newPoint.X].Destination = true;
                    this[(int)newPoint.Y][(int)newPoint.X].Passable = true;
                }
                else if (p.State == SensorState.Boundary && this[(int)newPoint.Y][(int)newPoint.X].Destination == false)
                    this[(int)newPoint.Y][(int)newPoint.X].Passable = false;
            }
        }
    }
}
