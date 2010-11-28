using System;
using System.Drawing;

namespace MultiAgentLibrary
{
    public class FieldSquare
    {
        public const int SuccessPheremoneLevel = 1000;
        public const int PheremoneDecayRate = (SuccessPheremoneLevel) / 1000;

        private uint pheromoneLevel;

        public const uint MaxPheremoneLevel = uint.MaxValue;

        public object LockObject = new object();

        public FieldSquare(Point position)
        {
            Position = position;
            PheremoneLevel = 1;
        }

        public Point Position { get; set; }

        public Color SquareColor
        {
            get
            {
                switch (Type)
                {
                    case SquareType.Wall:
                        return Color.Red;
                    case SquareType.Destination:
                        return Color.White;
                    default:
                        var level = (byte)(Math.Round((255.0 / Math.Log(MaxPheremoneLevel)) * Math.Log(pheromoneLevel)));
                        return Color.FromArgb(0, level, 0);
                }
            }
        }

        public uint PheremoneLevel
        {
            get
            {
                return pheromoneLevel;
            }
            set
            {
                if (value > MaxPheremoneLevel)
                    pheromoneLevel = MaxPheremoneLevel;
                else if (value < 0)
                    pheromoneLevel = 0;
                else
                    pheromoneLevel = value;
            }
        }

        private SquareType type;

        public SquareType Type
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
                        PheremoneLevel = 0;
                        break;
                    case SquareType.Destination:
                        PheremoneLevel = uint.MaxValue;
                        break;
                }
            }
        }
    }
}
