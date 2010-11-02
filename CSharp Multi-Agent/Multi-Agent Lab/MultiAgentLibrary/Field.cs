using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MultiAgentLibrary
{
    using System.Threading;
    using System.Drawing;

    public class Field : ObservableCollection<FieldRow>
    {
        public Point StartPoint { get; set; }

        public List<Agent> AgentsList { get; private set; }

        public Field(int width) : this(width, width) { }

        public Field(int width, int height)
        {
            AgentsList = new List<Agent>();
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
            var lines = File.ReadAllLines(filename);

            // Work out the starting point.
            var firstLineParts = lines.First().Split(new[] { "," }, 3, StringSplitOptions.RemoveEmptyEntries);
            var origStartPoint = new PointF(float.Parse(firstLineParts[0]), float.Parse(firstLineParts[1]));

            // Split each line up in to a list of readings, then merge them all together.
            // Don't care about order, so can be done in parallel.
            var rawPoints = lines.AsParallel().SelectMany(l =>
                {
                    var parts = l.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var origin = new PointF(float.Parse(parts[0]), float.Parse(parts[1]));

                    var output = new List<SensorReading>();
                    for (var i = 2; i < parts.Length; i += 3)
                    {
                        var reading = new SensorReading
                        {
                            Angle = float.Parse(parts[i]),
                            Range = float.Parse(parts[i + 1]),
                            State = (SensorState)Enum.Parse(typeof(SensorState), parts[i + 2], true),
                            Origin = origin
                        };

                        output.Add(reading);
                    }
                    return output;
                }).Select(r => new
                                    {
                                        Point = new PointF(r.Origin.X + r.Range * (float)Math.Sin(r.Angle), r.Origin.Y + r.Range * (float)Math.Cos(r.Angle)),
                                        r.State
                                    });

            // Work out the dimensions of the map
            var maxX = rawPoints.Max(p => p.Point.X);
            var maxY = rawPoints.Max(p => p.Point.Y);

            // Get the size of each new map square
            var yRectSize = maxY / (Count - 1);
            var xRectSize = maxX / (this.First().Count - 1);

            // Scale the starting point
            StartPoint = new Point((int)Math.Round(origStartPoint.X / xRectSize), (int)Math.Round(origStartPoint.Y / yRectSize));

            // Process each sensor point and transfer to the new map.
            // Can't do this parallel.  Too much chance of race conditions.
            foreach (var p in rawPoints.ToList())
            {
                var newPoint = new Point((int)Math.Round(p.Point.X / xRectSize), (int)Math.Round(p.Point.Y / yRectSize));

                if (p.State == SensorState.End)
                {
                    this[newPoint.Y][newPoint.X].Destination = true;
                    this[newPoint.Y][newPoint.X].Passable = true;
                }
                else if (p.State == SensorState.Boundary && this[newPoint.Y][newPoint.X].Destination == false)
                    this[newPoint.Y][newPoint.X].Passable = false;
            }
        }

        public void CycleAgents()
        {
            var result = Parallel.ForEach(AgentsList, agent => agent.Process(this));

            while (!result.IsCompleted)
                Thread.Sleep(1);

            result = Parallel.ForEach(this.SelectMany(row => row.Where(square => square.PheremoneLevel > 1 && square.Destination == false)),
                square =>
                {
                    square.PheremoneLevel -= FieldSquare.PheremoneDecayRate;
                });

            while (!result.IsCompleted)
                Thread.Sleep(1);
        }
    }
}
