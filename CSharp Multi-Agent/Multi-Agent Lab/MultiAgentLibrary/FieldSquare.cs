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
using System.Drawing;
using System.Xml.Serialization;

namespace MultiAgentLibrary
{
    public class FieldSquare
    {
        public const int SuccessPheromoneLevel = 1000;
        public const int PheromoneDecayRate = (SuccessPheromoneLevel) / 1000;

        private uint pheromoneLevel;

        public const uint MaxPheromoneLevel = uint.MaxValue;

        private readonly object lockObject = new object();

        public FieldSquare()
        {
        }

        public FieldSquare(Point position)
        {
            Position = position;
            PheromoneLevel = 1;
        }

        [XmlElement("POS")]
        public Point Position { get; set; }

        [XmlIgnore]
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

        [XmlIgnore]
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

        [XmlAttribute]
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
                    default:
                        PheromoneLevel = 1;
                        break;
                }
            }
        }

        [XmlIgnore]
        public object LockObject
        {
            get
            {
                return lockObject;
            }
        }
    }
}
