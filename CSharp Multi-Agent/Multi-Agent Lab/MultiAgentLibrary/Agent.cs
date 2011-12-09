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
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;

namespace MultiAgentLibrary
{
    public class Agent
    {
        private readonly Point startPosition;

        public Point Position { get; set; }

        public bool FoundEnd { get; set; }

        public int ShortTermMemoryLength { get; set; }

        private readonly List<Point> pastRoute = new List<Point>();
        private readonly HashSet<Point> pastRouteHash = new HashSet<Point>();

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
                        Console.WriteLine("Agent Route Length : {0}", pastRoute.Count);
                    }
                }

                Position = startPosition;
                pastRoute.Clear();
                pastRouteHash.Clear();
            }
            else
            {
                var recentSquares = pastRoute.Count >= ShortTermMemoryLength ? pastRoute.GetRange(pastRoute.Count - ShortTermMemoryLength, ShortTermMemoryLength) : pastRoute;

                var squares = new[] { field.Squares[currentIndex - field.Width], // Square above
                                      field.Squares[currentIndex + field.Width], // Square below
                                      field.Squares[currentIndex - 1],           // Square to the left
                                      field.Squares[currentIndex + 1],           // Square to the right

                                      /*field.Squares[currentIndex - field.Width - 1], // Square above and to the left
                                      field.Squares[currentIndex - field.Width + 1], // Square above and to the right
                                      field.Squares[currentIndex + field.Width - 1], // Square below and to the left
                                      field.Squares[currentIndex + field.Width + 1], // Square below and to the right*/
                };

                var biasedSquares = squares.Select(s => new { Square = s, Bias = (recentSquares.Contains(s.Position)) ? 0.25 : 1.0 });
                var weightedSquares = biasedSquares.Select(s => new { Square = s.Square, Weight = s.Bias * s.Square.PheromoneLevel }).ToList();
                var totalWeight = weightedSquares.Sum(s => s.Weight);

                var randNum = rand.NextDouble() * totalWeight;

                foreach (var s in weightedSquares)
                {
                    if (randNum < s.Weight)
                    {
                        Position = s.Square.Position;
                        break;
                    }
                    else
                    {
                        randNum -= s.Weight;
                    }
                }

                RememberRoute(currentSquare.Position);
            }
        }

        private void RememberRoute(Point currentSquare)
        {
            if (pastRouteHash.Contains(currentSquare))
            {
                Debug.Assert(pastRoute.Contains(currentSquare));
                // Already been here. All the exploring since was pointless...
                var index = pastRoute.IndexOf(currentSquare);
                var subList = pastRoute.Skip(index + 1);
                foreach (var r in subList)
                    pastRouteHash.Remove(r);
                pastRoute.RemoveRange(index + 1, pastRoute.Count - (index + 1));
                
            }
            else
            {
                Debug.Assert(!pastRoute.Contains(currentSquare));
                pastRoute.Add(currentSquare);
                pastRouteHash.Add(currentSquare);
            }
        }
    }
}
