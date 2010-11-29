﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace MultiAgentLibrary
{
    public class Agent
    {
        private readonly Point startPosition;

        public Point Position { get; set; }

        public bool FoundEnd { get; set; }

        public int ShortTermMemoryLength { get; set; }

        private readonly List<Point> pastRoute = new List<Point>();

        private readonly Random rand = new Random();

        public Agent(Point startPosition, int memoryLength)
        {
            Position = startPosition;
            ShortTermMemoryLength = memoryLength;
            this.startPosition = startPosition;
            FoundEnd = false;
        }

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