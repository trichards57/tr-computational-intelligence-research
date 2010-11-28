using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MultiAgentLibrary
{
    using System.Drawing;
    using System.Threading.Tasks;

    public class Field
    {
        public Point StartPoint { get; set; }

        public List<Agent> AgentsList { get; private set; }

        public List<Point> OriginalRoute { get; private set; }

        public List<Point> ShortestRoute { get; private set; }

        private readonly FieldSquare[] squares;
        public ReadOnlyCollection<FieldSquare> Squares
        {
            get
            {
                return new ReadOnlyCollection<FieldSquare>(squares);
            }
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public SizeF SquareSize { get; set; }

        public Field(int width) : this(width, width) { }

        public Field(int width, int height)
        {
            AgentsList = new List<Agent>();
            squares = new FieldSquare[width * height];
            OriginalRoute = new List<Point>();
            ShortestRoute = new List<Point>();

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    squares[x + y * width] = new FieldSquare(new Point(x, y));
                }
            }
            Width = width;
            Height = height;
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

            var unscaledOriginalRoute = new List<PointF>();

            // Split each line up in to a list of readings, then merge them all together.
            // Don't care about order, so can be done in parallel.

            var rawPoints = lines.AsParallel().SelectMany(l =>
                {
                    var parts = l.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var origin = new PointF(float.Parse(parts[0]), float.Parse(parts[1]));

                    unscaledOriginalRoute.Add(origin);

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
            var yRectSize = maxY / (height - 1);
            var xRectSize = maxX / (width - 1);
            SquareSize = new SizeF(xRectSize, yRectSize);

            // Scale the starting point
            StartPoint = new Point((int)Math.Round(origStartPoint.X / xRectSize), (int)Math.Round(origStartPoint.Y / yRectSize));

            OriginalRoute.AddRange(unscaledOriginalRoute.Select(p => new Point((int)Math.Round(p.X / xRectSize), (int)Math.Round(p.Y / yRectSize))));

            Parallel.ForEach(rawPoints, p =>
                {
                    var newPoint = new Point((int)Math.Round(p.Point.X / xRectSize), (int)Math.Round(p.Point.Y / yRectSize));

                    var index = newPoint.X + newPoint.Y * height;

                    lock (squares[index].LockObject)
                    {
                        if (p.State == SensorState.End)
                        {
                            squares[index].Type = SquareType.Destination;
                        }
                        else if (p.State == SensorState.Boundary && squares[index].Type != SquareType.Destination)
                            squares[index].Type = SquareType.Wall;
                    }
                });
        }

        public void CycleAgents()
        {
            Parallel.ForEach(AgentsList, agent => agent.Process(this));

            Parallel.ForEach(squares.Where(square => square.PheremoneLevel > 1 && square.Type == SquareType.Passable),
                square =>
                {
                    square.PheremoneLevel -= FieldSquare.PheremoneDecayRate;
                });
        }
    }
}
