using System;
using System.ComponentModel;
using System.Drawing;

namespace MultiAgentLibrary
{
    public class FieldSquare : INotifyPropertyChanged
    {
        public const int SuccessPheremoneLevel = 1000;
        public const int PheremoneDecayRate = (SuccessPheremoneLevel) / 1000;

        private Point position;
        private uint pheromoneLevel;
        private bool passable = true;
        private bool destination;

        public const uint MaxPheremoneLevel = uint.MaxValue;

        public FieldSquare(Point position)
        {
            Position = position;
            PheremoneLevel = 1;
        }

        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                OnPropertyChanged("Position");
            }
        }

        public Color SquareColor
        {
            get
            {
                if (!passable)
                    return Color.Red;
                if (destination)
                    return Color.White;

                var level = (byte)(Math.Round((255.0 / Math.Log(MaxPheremoneLevel)) * Math.Log(pheromoneLevel)));
                return Color.FromArgb(0, level, 0);
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

                OnPropertyChanged("PheremoneLevel");
                OnPropertyChanged("SquareColor");
            }
        }

        public bool Passable
        {
            get
            {
                return passable;
            }
            set
            {
                passable = value;
                if (!value)
                    PheremoneLevel = 0;
                OnPropertyChanged("SquareColor");
                OnPropertyChanged("Passable");
            }
        }

        public bool Destination
        {
            get
            {
                return destination;
            }
            set
            {
                destination = value;
                if (destination)
                {
                    Passable = true;
                    PheremoneLevel = MaxPheremoneLevel;
                }
                OnPropertyChanged("SquareColor");
                OnPropertyChanged("Destination");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
