//***********************************************************************
// Assembly         : FieldTidier
// Author           : Tony Richards
// Created          : 08-18-2011
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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using MultiAgentLibrary;
using System.Drawing;

namespace FieldTidier
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            var options = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine };
            var deserializer = new XmlSerializer(typeof(Field));
            var stream = XmlReader.Create(@"C:\Users\Tony\Documents\CIResearch\CSharp Multi-Agent\Multi-Agent Lab\MultiAgentConsole\bin\Debug\field.xml");
            var field = (Field)deserializer.Deserialize(stream);
            stream.Close();

            var newOriginalRoute = new List<Point>();
            foreach (var p in field.OriginalRoute)
            {
                if (newOriginalRoute.Contains(p))
                {
                    var index = newOriginalRoute.IndexOf(p);
                    newOriginalRoute.RemoveRange(index + 1, newOriginalRoute.Count - (index + 1));
                }
                else
                {
                    newOriginalRoute.Add(p);
                }
            }
            field.OriginalRoute = newOriginalRoute;

            var outStream = XmlWriter.Create(@"C:\Users\Tony\Documents\CIResearch\CSharp Multi-Agent\Multi-Agent Lab\MultiAgentConsole\bin\Debug\field.xml", options);
            deserializer.Serialize(outStream, field);
            outStream.Close();
        }
    }
}
