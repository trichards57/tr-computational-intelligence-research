using System;
using System.Drawing;

namespace MultiAgentLibrary
{
    public class FieldSquare
    {
        public const int SuccessPheromoneLevel = 1000;
        public const int PheromoneDecayRate = (SuccessPheromoneLevel) / 1000;

        private uint pheromoneLevel;

        public const uint MaxPheromoneLevel = uint.MaxValue;

        private readonly object lockObject = new object();

        public FieldSquare(Point position)
        {
            Position = position;
            PheromoneLevel = 1;
        }

        public Point Position { get; set; }

        public Color SquareColour
        {
            get
            {
                switch (SquareType)
                {
                    case SquareType.Wall:
                        return Color.Red;
                    case SquareType.Destination:
                        return Color.White;
                    default:
                        var level = (byte)(Math.Round((255.0 / Math.Log(MaxPheromoneLevel)) * Math.Log(pheromoneLevel)));
                        return Color.FromArgb(0, level, 0);
                }
            }
        }

        public uint PheromoneLevel
        {
            get
            {
                return pheromoneLevel;
            }
            set
            {
                if (value > MaxPheromoneLevel)
                    pheromoneLevel = MaxPheromoneLevel;
                else if (value < 0)
                    pheromoneLevel = 0;
                else
                    pheromoneLevel = value;
            }
        }

        private SquareType type;

        public SquareType SquareType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                switch (type)
                {
                    case SquareType.Wall:
                        PheromoneLevel = 0;
                        break;
                    case SquareType.Destination:
                        PheromoneLevel = uint.MaxValue;
                        break;
                }
            }
        }

        public object LockObject
        {
            get
            {
                return lockObject;
            }
        }
    }
}
