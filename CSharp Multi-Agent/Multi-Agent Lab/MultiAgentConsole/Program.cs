//***********************************************************************
// Assembly         : MultiAgentConsole
// Author           : Tony Richards
// Created          : 08-15-2011
//
// Last Modified By : Tony Richards
// Last Modified On : 08-29-2011
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using MultiAgentLibrary;

using UsefulClasses;
using System.Globalization;

namespace MultiAgentConsole
{
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using System.Xml;
    using UsefulClasses.Exceptions;

    internal sealed class Program
    {
        static ParameterManager parameterManager = new ParameterManager();

        static int Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Multi-Agent Lab Console : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");
            if (args.Length < 1)
            {
                WriteInstructions();
                return 1;
            }

            var dataFileParameter = new Parameter<string>("d", null, s => s) { Description = "The filename of a CSV file containing sensor readings.", FriendlyName = "Sensor Data File", Required = true };
            var cacheFileParameter = new Parameter<string>("cf", "field.xml", s => s) { Description = "The filename of an XML file to use as a cache for the processed sensor readings.", FriendlyName = "Sensor Data Cache File" };
            var outputFileParameter = new Parameter<string>("of", "route.csv", s => s) { Description = "The filename of a CSV file containing the output route.", FriendlyName = "Route File" };
            var outputImageParameter = new Parameter<string>("oi", "output.gif", s => s) { Description = "The filename of a GIF file containing the routine result shown as an image.", FriendlyName = "Output Image" };
            var mapWidthParameter = new Parameter<int>("w", 100, s => int.Parse(s, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture)) { Description = "An integer specifying the desired width of the map.", FriendlyName = "Map Width" };
            var mapHeightParameter = new Parameter<int>("h", 100, s => int.Parse(s, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture)) { Description = "An integer specifying the desired height of the map.", FriendlyName = "Map Height" };
            var maxAgentsParameter = new Parameter<int>("ma", 250, s => int.Parse(s, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture)) { Description = "An integer specifying the maximum number of agents for the simulation.", FriendlyName = "Maximum Agents" };
            var startAgentsParameter = new Parameter<int>("sa", 1, s => int.Parse(s, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture)) { Description = "An integer specifying the maximum number of agents for the simulation.", FriendlyName = "Starting Agents" };
            var cycleCountParameter = new Parameter<int>("c", 40000, s => int.Parse(s, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture)) { Description = "An integer specifying the number of cycles the sequence will run for.", FriendlyName = "Cycle Count" };
            var memoryLengthParameter = new Parameter<int>("sm", 4, s => int.Parse(s, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture)) { Description = "An integer specifying the length of the agent's short term route memory, which is used to prevent back tracking.", FriendlyName = "Short Term Memory Length" };

            var snapshotIntervalParameter = new Parameter<int>("si", -1, s => int.Parse(s, CultureInfo.InvariantCulture, CultureInfo.InvariantCulture)) { Description = "The interval that a snapshot should be taken at. -1 disables the snapshots.", FriendlyName = "Snapshot Interval" };

            parameterManager.RegisterParameter(dataFileParameter);
            parameterManager.RegisterParameter(cacheFileParameter);
            parameterManager.RegisterParameter(outputFileParameter);
            parameterManager.RegisterParameter(outputImageParameter);
            parameterManager.RegisterParameter(mapWidthParameter);
            parameterManager.RegisterParameter(mapHeightParameter);
            parameterManager.RegisterParameter(maxAgentsParameter);
            parameterManager.RegisterParameter(startAgentsParameter);
            parameterManager.RegisterParameter(cycleCountParameter);
            parameterManager.RegisterParameter(memoryLengthParameter);
            parameterManager.RegisterParameter(snapshotIntervalParameter);

            try
            {
                parameterManager.ProcessParameters(args);
            }
            catch (InvalidParameterException ex)
            {
                var str = string.Format(CultureInfo.CurrentCulture, "Unable to process command line arguments : {0}", ex.Message);
                var parts = str.Wrap(Console.WindowWidth);
                
                foreach (var p in parts)
                    Console.WriteLine(p);

                Console.WriteLine();
                WriteInstructions();
                return 1;
            }

            var dataFile = dataFileParameter.Value;
            var cacheFile = cacheFileParameter.Value;
            var mapWidth = mapWidthParameter.Value;
            var mapHeight = mapHeightParameter.Value;
            var maxAgents = maxAgentsParameter.Value;
            var startAgents = startAgentsParameter.Value;
            var cycleCount = cycleCountParameter.Value;
            var memoryLength = memoryLengthParameter.Value;
            var outputFile = outputFileParameter.Value;
            var outputImage = outputImageParameter.Value;
            var frameSkip = snapshotIntervalParameter.Value;

            if (!File.Exists(dataFile))
            {
                Console.WriteLine("Data file {0} does not exist.", dataFile);
                return 2;
            }

            Console.WriteLine(parameterManager.GenerateParameterStatusMessage());

            Console.WriteLine("Loading data file...");
            Field field;
            if (File.Exists(cacheFile) && File.GetLastWriteTimeUtc(cacheFile) > File.GetLastWriteTimeUtc(dataFile))
            {
                Console.WriteLine("Loading data from cache...");
                var deserializer = new XmlSerializer(typeof(Field));
                var stream = XmlReader.Create(cacheFile);
                field = (Field)deserializer.Deserialize(stream);
                stream.Close();
            }
            else
            {
                field = new Field(mapWidth, mapHeight, dataFile);
                Console.WriteLine("Caching field data...");
                var serializer = new XmlSerializer(typeof(Field));
                var stream = XmlWriter.Create(cacheFile);
                serializer.Serialize(stream, field);
                stream.Close();
            }
            Console.WriteLine("Data file loaded.");
            Console.WriteLine();

            Console.WriteLine("Setting up initial agents...");
            for (var i = 0; i < startAgents; i++)
                field.AgentsList.Add(new Agent(field.StartPoint, memoryLength));
            Console.WriteLine("Initial agents set up.");
            Console.WriteLine();

            Console.WriteLine("Starting simulation.");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < cycleCount; i++)
            {
                if ((i + 1) % 10 == 0 && field.AgentsList.Count < maxAgents)
                {
                    field.AgentsList.Add(new Agent(field.StartPoint, memoryLength));
                }
                field.CycleAgents();

                if (frameSkip != -1 && i % frameSkip == 0)
                {
                    var oImage = new System.Drawing.Bitmap(mapWidth * 10, mapHeight * 10);
                    using (var graphics = System.Drawing.Graphics.FromImage(oImage))
                    {
                        foreach (var square in field.Squares)
                        {
                            var col = square.SquareColour;
                            graphics.FillRectangle(new SolidBrush(col), (int)square.Position.X * 10, (int)square.Position.Y * 10, 10, 10);
                        }
                        foreach (var agent in field.AgentsList)
                        {
                            graphics.FillEllipse(Brushes.Yellow, (int)agent.Position.X * 10, (int)agent.Position.Y * 10, 10, 10);
                        }
                    }

                    Directory.CreateDirectory(@".\Frames");

                    oImage.Save(string.Format(CultureInfo.CurrentCulture, @".\Frames\output{0:00000000}.png", i / frameSkip), ImageFormat.Png);
                    oImage.Dispose();
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Simulation stopped.  Total Time : {0} seconds", stopwatch.Elapsed.TotalSeconds);
            Console.WriteLine();

            Console.WriteLine("Outputing final state image.");

            var outputBitmap = new Bitmap(mapWidth * 10, mapHeight * 10);
            using (var graphics = Graphics.FromImage(outputBitmap))
            {

                foreach (var square in field.Squares)
                {
                    var col = square.SquareColour;
                    graphics.FillRectangle(new SolidBrush(col), square.Position.X * 10, square.Position.Y * 10, 10, 10);
                }
                foreach (var point in field.OriginalRoute)
                {
                    graphics.FillRectangle(Brushes.Magenta, point.X * 10 + 5, point.Y * 10 + 5, 5, 5);
                }
                foreach (var point in field.ShortestRoute)
                {
                    graphics.FillEllipse(Brushes.Yellow, point.X * 10 + 2.5f, point.Y * 10 + 2.5f, 5.0f, 5.0f);
                }
            }

            outputBitmap.Save(outputImage, ImageFormat.Gif);

            Console.WriteLine("Image written.");

            Console.WriteLine("Removing uneeded route points");
            var scaledData = field.ShortestRoute.Select(p => new PointF(p.X * field.SquareSize.Width, p.Y * field.SquareSize.Height)).ToList();

            var result = new List<PointF>();

            for (var i = 0; i < scaledData.Count; i++)
            {
                if (result.Count < 2)
                    result.Add(scaledData[i]);
                else
                {
                    var p1 = result[result.Count - 2];
                    var p2 = result[result.Count - 1];

                    var m = (p1.Y - p2.Y) / (p1.X - p2.X);
                    var c = p1.Y - m * p1.X;

                    if (float.IsInfinity(m) && scaledData[i].X == p1.X)
                        result[result.Count - 1] = scaledData[i];
                    else if (m == 0 && scaledData[i].Y == p1.Y)
                        result[result.Count - 1] = scaledData[i];
                    else if (m * scaledData[i].X + c == scaledData[i].Y)
                        result[result.Count - 1] = scaledData[i];
                    else
                        result.Add(scaledData[i]);
                }
            }

            Console.WriteLine("Writing route data file.");
            using (var file = new StreamWriter(File.OpenWrite(outputFile)))
            {
                foreach (var p in result)
                {
                    file.WriteLine("{0},{1}", p.X, p.Y);
                }
            }
            Console.WriteLine("Shortest Route Length : {0}", field.ShortestRoute.Count);
            Console.WriteLine("Finished...");

            Console.ReadLine();
            return 0;
        }

        static void WriteInstructions()
        {
            Console.WriteLine(parameterManager.GenerateCommandLineUsageMessage("MultiAgentConsole.exe"));

            Console.WriteLine("Press ENTER to exit...");

            Console.ReadLine();
        }
    }
}
