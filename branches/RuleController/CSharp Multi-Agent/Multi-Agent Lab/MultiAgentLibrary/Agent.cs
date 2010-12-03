using System;
using System.Collections.Generic;
using System.Drawing;

namespace MultiAgentLibrary
{
    /// <summary>
    /// Represents an agent that moves through the environment.
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// The position the agent starts at in the environment, and where it returns to when it
        /// has found the end.
        /// </summary>
        private readonly Point startPosition;

        /// <summary>
        /// Gets or sets the current position of the agent.
        /// </summary>
        /// <value>The position of the agent.</value>
        public Point Position { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the agent has found the destination.
        /// </summary>
        /// <value><c>true</c> if it has found the end; otherwise, <c>false</c>.</value>
        public bool FoundEnd { get; set; }

        /// <summary>
        /// Gets or sets the length of the Agent's short term memory.
        /// </summary>
        /// <value>The length of the Agent's term memory.</value>
        public int ShortTermMemoryLength { get; set; }

        /// <summary>
        /// A list containing all the points on the Agent's route.
        /// </summary>
        private readonly List<Point> pastRoute = new List<Point>();

        private readonly Random rand = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="Agent"/> class.
        /// </summary>
        /// <param name="startPosition">The agent's start position.</param>
        /// <param name="memoryLength">Length of the agent's short-term memory.</param>
        public Agent(Point startPosition, int memoryLength)
        {
            Position = startPosition;
            ShortTermMemoryLength = memoryLength;
            this.startPosition = startPosition;
            FoundEnd = false;
        }

        /// <summary>
        /// Triggers the agent to make a movement through the field in <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The field the agent is currently in.</param>
        /// <remarks>
        /// The agent decides which way to move based on the level of pheremone in the squares.
        /// The square receives a heavy penality if it has been visited during the agent's
        /// short term memory.
        /// 
        /// The agent assumes that the outside border locations of the field are outside the maze.  
        /// Therefore, if it finds itself in these locations, it assumes it has somehow left the
        /// maze and resets itself back to the start.
        /// </remarks>
        public void Process(Field field)
        {
            var x = Position.X;
            var y = Position.Y;

            if (x == 0 || x == field.Width - 1 || y == 0 || y == field.Height - 1)
            {
                Position = startPosition;
                Console.WriteLine("Agent got itself stuck!");
                pastRoute.Clear();

                x = Position.X;
                y = Position.Y;
            }

            var currentIndex = x + y * field.Width;

            var currentSquare = field.Squares[currentIndex];
            var upSquare = field.Squares[currentIndex - field.Width];
            var downSquare = field.Squares[currentIndex + field.Width];
            var leftSquare = field.Squares[currentIndex - 1];
            var rightSquare = field.Squares[currentIndex + 1];

            if (currentSquare.SquareType == SquareType.Destination)
            {
                var sizeScale = (1000 / pastRoute.Count) * 0.1;
                lock (field)
                {
                    foreach (var p in pastRoute)
                    {

                        field.Squares[p.X + field.Width * p.Y].PheromoneLevel += (uint)(FieldSquare.SuccessPheromoneLevel * sizeScale);
                    }

                    if (pastRoute.Count < field.ShortestRoute.Count || field.ShortestRoute.Count == 0)
                    {
                        field.ShortestRoute.Clear();
                        foreach (var p in pastRoute)
                            field.ShortestRoute.Add(p);
                    }
                }

                Position = startPosition;
                Console.WriteLine("Agent Route Length : {0}", pastRoute.Count);
                pastRoute.Clear();
            }
            else
            {
                var recentSquares = pastRoute.Count >= ShortTermMemoryLength ? pastRoute.GetRange(pastRoute.Count - ShortTermMemoryLength, ShortTermMemoryLength) : pastRoute;
                var upBias = 1.0;
                var downBias = 1.0;
                var leftBias = 1.0;
                var rightBias = 1.0;

                // Trying to encourage movement away from the home.
                if (recentSquares.Contains(upSquare.Position))
                    upBias = 0.25;
                if (recentSquares.Contains(downSquare.Position))
                    downBias = 0.25;
                if (recentSquares.Contains(leftSquare.Position))
                    leftBias = 0.25;
                if (recentSquares.Contains(rightSquare.Position))
                    rightBias = 0.25;

                var up = upSquare.PheromoneLevel * upBias;
                var down = downSquare.PheromoneLevel * downBias;
                var left = leftSquare.PheromoneLevel * leftBias;
                var right = rightSquare.PheromoneLevel * rightBias;

                var totalWeight = up + down + left + right;
                var randNum = rand.NextDouble() * totalWeight;

                if (randNum < up)
                    // Move up
                    Position = upSquare.Position;
                else if (randNum < (up + down))
                    // Move down
                    Position = downSquare.Position;
                else if (randNum < (up + down + left))
                    // Move left
                    Position = leftSquare.Position;
                else
                    // Move right
                    Position = rightSquare.Position;

                RememberRoute(currentSquare.Position);
            }
        }

        /// <summary>
        /// Adds the next point to the remembered the route.
        /// </summary>
        /// <param name="currentSquare">The point to add.</param>
        /// <remarks>
        /// If the point has never been visited before, this routine just adds it to the list.
        /// If the point has already been visited, the routine assumes that the agent has just
        /// gone through a loop.  All of the points stored during that loop are removed, smoothing
        /// the path out a little.
        /// </remarks>
        private void RememberRoute(Point currentSquare)
        {
            if (pastRoute.Contains(currentSquare))
            {
                // Already been here. All the exploring since was pointless...
                var index = pastRoute.IndexOf(currentSquare);
                pastRoute.RemoveRange(index + 1, pastRoute.Count - (index + 1));
            }
            else
            {
                pastRoute.Add(currentSquare);
            }
        }
    }
}
