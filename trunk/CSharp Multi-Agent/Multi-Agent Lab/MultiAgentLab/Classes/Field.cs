using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;

namespace MultiAgentLab.Classes
{
    enum SensorState
    {
        Boundary,
        End,
        None
    }

    class Field : ObservableCollection<FieldRow>
    {
        public Point StartPoint;

        public Field(int width) : this(width, width) { }

        public Field(int width, int height)
        {
            for (var i = 0; i < height; i++)
            {
                Add(new FieldRow(i, width));
            }
        }

        public Field(int width, int height, string filename) : this(width, height)
        {
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

            var pointsList = new Dictionary<Point, SensorState>();

            var dataFile = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileReader = new StreamReader(dataFile);

            while (!fileReader.EndOfStream)
            {


                var line = fileReader.ReadLine();
                var parts = line.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                var originX = double.Parse(parts[0]);
                var originY = double.Parse(parts[1]);

                if (StartPoint == new Point(0,0))
                    StartPoint = new Point(originX, originY);

                for (var i = 2; i < parts.Length; i += 3)
                {
                    var ang = double.Parse(parts[i]);
                    var range = double.Parse(parts[i + 1]);

                    var x = originX + range * Math.Sin(ang);
                    var y = originY + range * Math.Cos(ang);

                    var point = new Point(x, y);
                    var state = (SensorState)Enum.Parse(typeof(SensorState), parts[i + 2], true);

                    if (pointsList.ContainsKey(point))
                    {
                        if (state == SensorState.Boundary && pointsList[point] == SensorState.None)
                            // Boundary overrides None
                            pointsList[point] = state;
                        else if (state == SensorState.End && pointsList[point] == SensorState.Boundary)
                            // End overrides Boundary
                            pointsList[point] = state;
                    }
                    else
                        pointsList.Add(point, state);
                }
            }

            var maxX = pointsList.Max(p => p.Key.X);
            var maxY = pointsList.Max(p => p.Key.Y);

            var yRectSize = maxY / (Count - 1);
            var xRectSize = maxX / (this.First().Count - 1);

            StartPoint.X = (int)Math.Round(StartPoint.X / xRectSize);
            StartPoint.Y = (int)Math.Round(StartPoint.Y / yRectSize);

            foreach (var p in pointsList)
            {
                var newPoint = new Point(Math.Round(p.Key.X / xRectSize), Math.Round(p.Key.Y / yRectSize));

                if (p.Value == SensorState.End)
                {
                    this[(int)newPoint.Y][(int)newPoint.X].Destination = true;
                    this[(int)newPoint.Y][(int)newPoint.X].Passable = true;
                }
                else if (p.Value == SensorState.Boundary && this[(int)newPoint.Y][(int)newPoint.X].Destination == false)
                    this[(int)newPoint.Y][(int)newPoint.X].Passable = false;
            }
        }
    }
}
