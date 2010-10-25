using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MultiAgentLab.Classes
{
    enum Directions
    {
        Up,
        Down,
        Left,
        Right
    }

    class Agent
    {
        private Point StartPosition;

        public Point Position { get; set; }

        public bool FoundEnd { get; set; }

        private List<Point> pastRoute = new List<Point>();

        private Random rand = new Random();

        public Agent(Point startPosition)
        {
            Position = startPosition;
            StartPosition = startPosition;
            FoundEnd = false;
        }

        public void Process(Field field)
        {
            var x = (int)Position.X;
            var y = (int)Position.Y;

            var currentSquare = field[y][x];
            var upSquare = field[y - 1][x];
            var downSquare = field[y + 1][x];
            var leftSquare = field[y][x - 1];
            var rightSquare = field[y][x + 1];

            if (currentSquare.Destination)
            {
                foreach (var p in pastRoute)
                {
                    field[(int)p.Y][(int)p.X].PheremoneLevel += 1.0;
                }
                Position = StartPosition;
                pastRoute.Clear();
            }
            else
            {
                var lastFourSquare = pastRoute.GetRange(pastRoute.Count - 4, 4);
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

                pastRoute.Add(currentSquare.Position);

                currentSquare.PheremoneLevel += 0.01;
            }
        }
    }
}
