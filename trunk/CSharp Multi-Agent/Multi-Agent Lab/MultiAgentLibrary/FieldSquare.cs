using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MultiAgentLibrary
{
    public class FieldSquare : INotifyPropertyChanged
    {
        private Point position;
        private double pheromoneLevel;
        private bool passable = true;
        private bool destination;
        private double travelTime;

        public const double MaxPheremoneLevel = int.MaxValue / 2;

        public FieldSquare(Point position)
        {
            Position = position;
            PheremoneLevel = 1.0;
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
                    return Colors.Red;
                if (destination)
                    return Colors.White;

                var level = (byte)(Math.Round((255.0 / Math.Log(MaxPheremoneLevel)) * Math.Log(pheromoneLevel)));
                return Color.FromRgb(0, level, 0);
            }
        }

        public double PheremoneLevel
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
                Passable = true;
                PheremoneLevel = MaxPheremoneLevel;
                OnPropertyChanged("SquareColor");
                OnPropertyChanged("Destination");
            }
        }

        public double TravelTime
        {
            get
            {
                return travelTime;
            }
            set
            {
                travelTime = value;
                OnPropertyChanged("TravelTime");
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
