//***********************************************************************
// Assembly         : MultiAgentLibrary
// Author           : Tony Richards
// Created          : 08-15-2011
//
// Last Modified By : Tony Richards
// Last Modified On : 08-18-2011
// Description      : 
//
// Copyright (c) 2011, Tony Richards
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list
// of conditions and the following disclaimer.
//
// Redistributions in binary form must reproduce the above copyright notice, this
// list of conditions and the following disclaimer in the documentation and/or other
// materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
// OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
// OF THE POSSIBILITY OF SUCH DAMAGE.
//***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MultiAgentLibrary
{
    using System.Drawing;
    using System.Threading.Tasks;
    using System.Globalization;
    using System.Xml.Serialization;

    [XmlRoot]
    public class Field
    {
        [XmlElement]
        public Point StartPoint { get; set; }

        [XmlIgnore]
        public List<Agent> AgentsList { get; set; }

        [XmlArray]
        [XmlArrayItem("P")]
        public List<Point> OriginalRoute { get; set; }

        [XmlIgnore]
        public List<Point> ShortestRoute { get; set; }

        [XmlArray]
        [XmlArrayItem("FS")]
        public FieldSquare[] Squares { get; set; }

        [XmlAttribute]
        public int Width { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlElement]
        public SizeF SquareSize { get; set; }

        public Field()
        {
            AgentsList = new List<Agent>();
            OriginalRoute = new List<Point>();
            ShortestRoute = new List<Point>();
        }

        public Field(int width) : this(width, width) { }

        public Field(int width, int height)
            : this()
        {
            Squares = new FieldSquare[width * height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    Squares[x + y * width] = new FieldSquare(new Point(x, y));
                }
            }
            Width = width;
            Height = height;
        }

        public Field(int width, int height, string fileName)
            : this(width, height)
        {
            if (width == 0)
                throw new ArgumentOutOfRangeException("width");
            if (height == 0)
                throw new ArgumentOutOfRangeException("height");

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

            // Read the file in to memory.
            // Convert to a list to make sure all the reading is done now, not on demand.
            var lines = File.ReadAllLines(fileName);

            // Work out the starting point.
            var firstLineParts = lines.First().Split(new[] { "," }, 3, StringSplitOptions.RemoveEmptyEntries);
            var origStartPoint = new PointF(float.Parse(firstLineParts[0], CultureInfo.InvariantCulture), float.Parse(firstLineParts[1], CultureInfo.InvariantCulture));

            var unscaledOriginalRoute = new List<PointF>();

            // Split each line up in to a list of readings, then merge them all together.
            // Don't care about order, so can be done in parallel.

            var rawPoints = lines.AsParallel().SelectMany(l =>
                {
                    var parts = l.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    var origin = new PointF(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture));

                    unscaledOriginalRoute.Add(origin);

                    var output = new List<SensorReading>();

                    for (var i = 2; i < parts.Length; i += 3)
                    {
                        var reading = new SensorReading
                        {
                            Angle = float.Parse(parts[i], CultureInfo.InvariantCulture),
                            Range = float.Parse(parts[i + 1], CultureInfo.InvariantCulture),
                            State = (SensorState)Enum.Parse(typeof(SensorState), parts[i + 2], true),
                            Origin = origin
                        };

                        output.Add(reading);
                    }
                    return output;
                }).Select(r => new
                                    {
                                        Point = new PointF(r.Origin.X + r.Range * (float)Math.Sin(r.Angle), r.Origin.Y + r.Range * (float)Math.Cos(r.Angle)),
                                        r.State
                                    });

            // Work out the dimensions of the map
            var maxX = rawPoints.Max(p => p.Point.X);
            var maxY = rawPoints.Max(p => p.Point.Y);

            // Get the size of each new map square
            var yRectSize = maxY / (height - 1);
            var xRectSize = maxX / (width - 1);
            SquareSize = new SizeF(xRectSize, yRectSize);

            // Scale the starting point
            StartPoint = new Point((int)Math.Round(origStartPoint.X / xRectSize), (int)Math.Round(origStartPoint.Y / yRectSize));

            OriginalRoute.AddRange(unscaledOriginalRoute.Select(p => new Point((int)Math.Round(p.X / xRectSize), (int)Math.Round(p.Y / yRectSize))));

            Parallel.ForEach(rawPoints, p =>
                {
                    var newPoint = new Point((int)Math.Round(p.Point.X / xRectSize), (int)Math.Round(p.Point.Y / yRectSize));

                    var index = newPoint.X + newPoint.Y * height;

                    lock (Squares[index].LockObject)
                    {
                        if (p.State == SensorState.End)
                        {
                            Squares[index].SquareType = SquareType.Destination;
                        }
                        else if (p.State == SensorState.Boundary && Squares[index].SquareType != SquareType.Destination)
                            Squares[index].SquareType = SquareType.Wall;
                    }
                });
        }

        public void CycleAgents()
        {
            Parallel.ForEach(AgentsList, agent => agent.Process(this));

            Parallel.ForEach(Squares.Where(square => square.PheromoneLevel > 1 && square.SquareType == SquareType.Passable),
                square =>
                {
                    square.PheromoneLevel -= FieldSquare.PheromoneDecayRate;
                });
        }
    }
}
