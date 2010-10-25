using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MultiAgentLab.Classes
{
    class FieldSquare : INotifyPropertyChanged
    {
        private Point position;
        private double pheromoneLevel;
        private bool passable = true;
        private bool destination = false;
        private double travelTime;

        private static Random rand = new Random();

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

        public System.Drawing.Color SquareColor
        {
            get
            {
                if (!passable)
                    return System.Drawing.Color.Red;
                else if (destination)
                    return System.Drawing.Color.White;
                else
                {
                    var level = (byte)(Math.Round((255.0 / 10.0) * pheromoneLevel));
                    return System.Drawing.Color.FromArgb(0, level, 0);
                }
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
                if (value > 10)
                    pheromoneLevel = 10;
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
