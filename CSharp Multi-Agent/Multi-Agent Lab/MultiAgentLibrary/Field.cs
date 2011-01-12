using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MultiAgentLibrary
{
    using System.Drawing;
    using System.Threading.Tasks;
    using System.Globalization;

    /// <summary>
    /// Represents the environment that agents move through.
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Gets or sets the point where agents start, and where they return after completing the maze.
        /// </summary>
        /// <value>The agent start point.</value>
        public Point StartPoint { get; set; }

        /// <summary>
        /// The backing field for the <see cref="AgentsList"/> property.
        /// </summary>
        private readonly List<Agent> agentsList;

        /// <summary>
        /// The list of agents used present in the Field.
        /// </summary>
        /// <value>The agents list.</value>
        public Collection<Agent> AgentsList
        {
            get
            {
                return new Collection<Agent>(agentsList);
            }
        }

        /// <summary>
        /// Backing field for the <see cref="ShortestRoute"/> property.
        /// </summary>
        private readonly List<Point> shortestRoute;

        /// <summary>
        /// Gets the shortest route produced by the agents.
        /// </summary>
        /// <value>The shortest route.</value>
        /// <remarks>Updated every cycle, whenever an agent completes the route.  Measured by the number
        /// of points in the route.</remarks>
        public Collection<Point> ShortestRoute
        {
            get
            {
                return new Collection<Point>(shortestRoute);
            }
        }

        /// <summary>
        /// Backing field for the <see cref="Squares"/> property.
        /// </summary>
        private readonly FieldSquare[] squares;
        /// <summary>
        /// Gets all of the squares that make up the environment.
        /// </summary>
        /// <value>The environment's squares.</value>
        public ReadOnlyCollection<FieldSquare> Squares
        {
            get
            {
                return new ReadOnlyCollection<FieldSquare>(squares);
            }
        }

        /// <summary>
        /// Gets or sets the width of the environment.
        /// </summary>
        /// <value>The width of the environment in squares.</value>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the environment.
        /// </summary>
        /// <value>The height of the environment in squares.</value>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the size of each square relative to the original map.
        /// </summary>
        /// <value>The size of the square in pixels.</value>
        public SizeF SquareSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class with the given <paramref name="width"/> and <paramref name="height"/>.
        /// </summary>
        /// <param name="width">The width of the environment.</param>
        /// <param name="height">The height of the environment.</param>
        /// <remarks>Generates an empty environment, <paramref name="width"/> squares across and <paramref name="height"/> squares down.</remarks>
        public Field(int width, int height)
        {
            agentsList = new List<Agent>();
            squares = new FieldSquare[width * height];
            shortestRoute = new List<Point>();

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class, with the given <paramref name="width"/> and
        /// <paramref name="height"/>, based on the given sensor data file.
        /// </summary>
        /// <param name="width">The width of the environment.</param>
        /// <param name="height">The height of the environment.</param>
        /// <param name="fileName">Name of the file containing sensor information.</param>
        /// <remarks>
        /// Loads the specified sensor data in <paramref name="fileName"/>.  Each row consists of an origin
        /// point, followed by a comma seperated list of ang, range and state information, producing a 
        /// results like:
        /// 
        /// <c>
        /// x,y,sensor0ang,sensor0val,sensor0state,sensor1ang,sensor1val,sensor1state,...,sensor39ang,sensor39val,sensor39state
        /// x,y,sensor0ang,sensor0val,sensor0state,sensor1ang,sensor1val,sensor1state,...,sensor39ang,sensor39val,sensor39state
        /// </c>
        /// 
        /// This is parsed in to a list of readings consisting of a starting point, a range, an angle and what the 
        /// sensor has read.  This is then converted in to a list of points and what was at them (e.g. a wall, the destination
        /// or nothing (the sensor didn't pick anything up in range)).  
        /// 
        /// The resulting map produced is then divided in to a grid with given <paramref name="width"/> and 
        /// <paramref name="height"/>.  If a square contains a point that reads as a destination, the entire
        /// square becomes a destination.  Otherwise, if the square contains a point that reads as a wall,
        /// the entire square becomes a wall.  The square is only considered passable if all of the points that
        /// fall inside of it are passable.  This produced a representation of the environment that has lower
        /// resolution, but also consumes less memory and is faster to process.
        /// 
        /// As far as possible, this process is operated in parallel to speed up processing.
        /// </remarks>
        public Field(int width, int height, string fileName)
            : this(width, height)
        {
            if (width == 0)
                throw new ArgumentOutOfRangeException("width");
            if (height == 0)
                throw new ArgumentOutOfRangeException("height");

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
            var lines = File.ReadAllLines(fileName);

            // Work out the starting point.
            var firstLineParts = lines.First().Split(new[] { "," }, 3, StringSplitOptions.RemoveEmptyEntries);
            var origStartPoint = new PointF(float.Parse(firstLineParts[0], CultureInfo.InvariantCulture), float.Parse(firstLineParts[1], CultureInfo.InvariantCulture));

            // Split each line up in to a list of readings, then merge them all together.
            // Don't care about order, so can be done in parallel.

            var rawPoints = lines.AsParallel().SelectMany(l =>
                {
                    var parts = l.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var origin = new PointF(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture));

                    var output = new List<SensorReading>();

                    for (var i = 2; i < parts.Length; i += 3)
                    {
                        var reading = new SensorReading
                        {
                            Angle = float.Parse(parts[i], CultureInfo.InvariantCulture),
                            Range = float.Parse(parts[i + 1], CultureInfo.InvariantCulture),
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

            Parallel.ForEach(rawPoints, p =>
                {
                    var newPoint = new Point((int)Math.Round(p.Point.X / xRectSize), (int)Math.Round(p.Point.Y / yRectSize));

                    var index = newPoint.X + newPoint.Y * height;

                    lock (squares[index].LockObject)
                    {
                        if (p.State == SensorState.End)
                        {
                            squares[index].SquareType = SquareType.Destination;
                        }
                        else if (p.State == SensorState.Boundary && squares[index].SquareType != SquareType.Destination)
                            squares[index].SquareType = SquareType.Wall;
                    }
                });
        }

        /// <summary>
        /// Runs every agent and square through one cycle.
        /// </summary>
        /// <remarks>
        /// Triggers each agent to run it's <see cref="Agent.Process"/> function.  This is done in parallel,
        /// and so the order that each agent will be processed in is not defined.  The behaviour of each agent
        /// can be and is affected by the behaviour of those that have already been processed.  This is intentional.
        /// 
        /// Once all of the agents have been processed, the pheremone level of each square is reduced by 
        /// <see cref="FieldSquare.PheromoneDecayRate"/>, provided the pheremone level is above 1 and the square is
        /// not a wall or a destination.  This is executed in parallel where possible, but this has no side-effects.
        /// </remarks>
        public void CycleAgents()
        {
            Parallel.ForEach(AgentsList, agent => agent.Process(this));

            Parallel.ForEach(squares.Where(square => square.PheromoneLevel > 1 && square.SquareType == SquareType.Passable),
                square =>
                {
                    square.PheromoneLevel -= FieldSquare.PheromoneDecayRate;
                });
        }
    }
}
