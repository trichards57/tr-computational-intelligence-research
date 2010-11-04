﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace MultiAgentLibrary
{
    public enum Directions
    {
        Up,
        Down,
        Left,
        Right
    }

    public class Agent
    {
        private readonly Point startPosition;

        public Point Position { get; set; }

        public bool FoundEnd { get; set; }

        private readonly List<Point> pastRoute = new List<Point>();

        private readonly Random rand = new Random();

        public Agent(Point startPosition)
        {
            Position = startPosition;
            this.startPosition = startPosition;
            FoundEnd = false;
        }

        public void Process(Field field)
        {
            var x = Position.X;
            var y = Position.Y;

            if (x == 0 || x == field.First().Count - 1 || x == 0 || x == field.Count - 1)
            {
                Position = startPosition;
                Console.WriteLine("Agent got itself stuck!");
                pastRoute.Clear();
            }

            var currentSquare = field[y][x];
            var upSquare = field[y - 1][x];
            var downSquare = field[y + 1][x];
            var leftSquare = field[y][x - 1];
            var rightSquare = field[y][x + 1];

            if (currentSquare.Destination)
            {
                var sizeScale = (1000 / pastRoute.Count) * 0.1;
                foreach (var p in pastRoute)
                {
                    lock (field)
                    {
                        field[p.Y][p.X].PheremoneLevel += (uint)(FieldSquare.SuccessPheremoneLevel * sizeScale);
                    }
                }
                Position = startPosition;
                Console.WriteLine("Agent Route Length : {0}", pastRoute.Count);
                pastRoute.Clear();
            }
            else
            {
                var lastFourSquare = pastRoute.Count > 3 ? pastRoute.GetRange(pastRoute.Count - 4, 4) : pastRoute;
                var upBias = 1.0;
                var downBias = 1.0;
                var leftBias = 1.0;
                var rightBias = 1.0;

                // Trying to encourage movement away from the home.
                if (lastFourSquare.Contains(upSquare.Position))
                    upBias = 0.25;
                if (lastFourSquare.Contains(downSquare.Position))
                    downBias = 0.25;
                if (lastFourSquare.Contains(leftSquare.Position))
                    leftBias = 0.25;
                if (lastFourSquare.Contains(rightSquare.Position))
                    rightBias = 0.25;

                var up = upSquare.PheremoneLevel * upBias;
                var down = downSquare.PheremoneLevel * downBias;
                var left = leftSquare.PheremoneLevel * leftBias;
                var right = rightSquare.PheremoneLevel * rightBias;

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
