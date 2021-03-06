﻿//***********************************************************************
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using MultiAgentLibrary;
using UsefulClasses;
using UsefulClasses.Exceptions;

namespace MultiAgentConsole
{
// ReSharper disable ClassNeverInstantiated.Global
    internal sealed class Program
// ReSharper restore ClassNeverInstantiated.Global
    {
        static readonly ParameterManager ParameterManager = new ParameterManager();

        static int Main(string[] args)
        {
            const double epsilon = 0.1;

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Multi-Agent Lab Console : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");
            
            var dataFileParameter = new Parameter<string>("d", null, s => s) { Description = "The filename of a CSV file containing sensor readings.", FriendlyName = "Sensor Data File", Required = true };
            var cacheFileParameter = new Parameter<string>("cf", "field.xml", s => s) { Description = "The filename of an XML file to use as a cache for the processed sensor readings.", FriendlyName = "Sensor Data Cache File" };
            var outputFileParameter = new Parameter<string>("of", "route.csv", s => s) { Description = "The filename of a CSV file containing the output route.", FriendlyName = "Route File" };
            var outputImageParameter = new Parameter<string>("oi", "output.gif", s => s) { Description = "The filename of a GIF file containing the routine result shown as an image.", FriendlyName = "Output Image" };
            var mapWidthParameter = new Parameter<int>("w", 100, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "An integer specifying the desired width of the map.", FriendlyName = "Map Width" };
            var mapHeightParameter = new Parameter<int>("h", 100, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "An integer specifying the desired height of the map.", FriendlyName = "Map Height" };
            var maxAgentsParameter = new Parameter<int>("ma", 250, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "An integer specifying the maximum number of agents for the simulation.", FriendlyName = "Maximum Agents" };
            var startAgentsParameter = new Parameter<int>("sa", 1, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "An integer specifying the maximum number of agents for the simulation.", FriendlyName = "Starting Agents" };
            var cycleCountParameter = new Parameter<int>("c", 40000, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "An integer specifying the number of cycles the sequence will run for.", FriendlyName = "Cycle Count" };
            var memoryLengthParameter = new Parameter<int>("sm", 4, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "An integer specifying the length of the agent's short term route memory, which is used to prevent back tracking.", FriendlyName = "Short Term Memory Length" };

            var snapshotIntervalParameter = new Parameter<int>("si", -1, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "The interval that a snapshot should be taken at. -1 disables the snapshots.", FriendlyName = "Snapshot Interval" };

            var outputSummeryParameter = new Parameter<string>("os", null, s => s) { Description = "The filename of an XML file used to store the output data for batch mode.", FriendlyName = "Output Summary File" };

            var batchModeParameter = new Parameter<bool>("bm", false, bool.Parse) { Description = "A boolean (true/false) specifying if the program should run in no-wait mode.", FriendlyName = "Batch Mode" };

            var xmlSnapshotIntervalParameter = new Parameter<int>("xi", -1, s => int.Parse(s, CultureInfo.InvariantCulture)) { Description = "The interval that an XML snapshot should be taken at. -1 disables the snapshots.", FriendlyName = "XML Snapshot Interval" };

            var disableCacheParameter = new Parameter<bool>("dc", false, bool.Parse) { Description = "A boolean (true/false) specifying if the program should cache map data.", FriendlyName = "Disable Cache" };

            ParameterManager.RegisterParameter(dataFileParameter);
            ParameterManager.RegisterParameter(cacheFileParameter);
            ParameterManager.RegisterParameter(outputFileParameter);
            ParameterManager.RegisterParameter(outputImageParameter);
            ParameterManager.RegisterParameter(mapWidthParameter);
            ParameterManager.RegisterParameter(mapHeightParameter);
            ParameterManager.RegisterParameter(maxAgentsParameter);
            ParameterManager.RegisterParameter(startAgentsParameter);
            ParameterManager.RegisterParameter(cycleCountParameter);
            ParameterManager.RegisterParameter(memoryLengthParameter);
            ParameterManager.RegisterParameter(snapshotIntervalParameter);
            ParameterManager.RegisterParameter(outputSummeryParameter);
            ParameterManager.RegisterParameter(batchModeParameter);
            ParameterManager.RegisterParameter(xmlSnapshotIntervalParameter);
            ParameterManager.RegisterParameter(disableCacheParameter);

            try
            {
                ParameterManager.ProcessParameters(args);
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
            var outputSummaryFile = outputSummeryParameter.Value;
            var batchMode = batchModeParameter.Value;
            var xmlSnapshotInterval = xmlSnapshotIntervalParameter.Value;
            var disableCache = disableCacheParameter.Value;

            if (!File.Exists(dataFile))
            {
                Console.WriteLine("Data file {0} does not exist.", dataFile);
                return 2;
            }

            Console.WriteLine(ParameterManager.GenerateParameterStatusMessage());

            Console.WriteLine("Loading data file...");
            Field field;
            if (!disableCache && File.Exists(cacheFile) && File.GetLastWriteTimeUtc(cacheFile) > File.GetLastWriteTimeUtc(dataFile))
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

            var snapshotList = new SnapshotCollection();

            Console.WriteLine("Starting simulation.");
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i <= cycleCount; i++)
            {
                if ((i + 1) % 10 == 0 && field.AgentsList.Count < maxAgents)
                {
                    field.AgentsList.Add(new Agent(field.StartPoint, memoryLength));
                }
                field.CycleAgents();

                if (frameSkip != -1 && i % frameSkip == 0)
                {
                    var oImage = new Bitmap(mapWidth * 10, mapHeight * 10);
                    using (var graphics = Graphics.FromImage(oImage))
                    {
                        foreach (var square in field.Squares)
                        {
                            var col = square.SquareColour;
                            graphics.FillRectangle(new SolidBrush(col), square.Position.X * 10, square.Position.Y * 10, 10, 10);
                        }
                        foreach (var agent in field.AgentsList)
                        {
                            graphics.FillEllipse(Brushes.Yellow, agent.Position.X * 10, agent.Position.Y * 10, 10, 10);
                        }
                    }

                    Directory.CreateDirectory(@".\Frames");

                    oImage.Save(string.Format(CultureInfo.CurrentCulture, @".\Frames\output{0:00000000}.png", i / frameSkip), ImageFormat.Png);
                    oImage.Dispose();
                }
                if (xmlSnapshotInterval != -1 && i % xmlSnapshotInterval == 0)
                {
                    if (outputSummaryFile != null)
                    {
                        snapshotList.Snapshots.Add(new Snapshot { CycleCount = i, RouteLength = field.ShortestRoute.Count, AgentCount = field.AgentsList.Count });
                    }
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

            foreach (PointF t in scaledData)
            {
                if (result.Count < 2)
                    result.Add(t);
                else
                {
                    var p1 = result[result.Count - 2];
                    var p2 = result[result.Count - 1];

                    var m = (p1.Y - p2.Y) / (p1.X - p2.X);
                    var c = p1.Y - m * p1.X;

                    if (float.IsInfinity(m) && Math.Abs(t.X - p1.X) < epsilon)
                        result[result.Count - 1] = t;
                    else if (Math.Abs(m - 0) < epsilon && Math.Abs(t.Y - p1.Y) < epsilon)
                        result[result.Count - 1] = t;
                    else if (Math.Abs(m * t.X + c - t.Y) < epsilon)
                        result[result.Count - 1] = t;
                    else
                        result.Add(t);
                }
            }

            Console.WriteLine("Writing route data file.");
            using (var file = new StreamWriter(File.Open(outputFile, FileMode.Create, FileAccess.Write)))
            {
                foreach (var p in result)
                {
                    file.WriteLine("{0},{1}", p.X, p.Y);
                }
            }
            Console.WriteLine("Shortest Route Length : {0}", field.ShortestRoute.Count);

            if (outputSummaryFile != null)
            {
                using (var file = File.Open(outputSummaryFile, FileMode.Create, FileAccess.Write))
                {
                    var writer = new XmlSerializer(typeof(SnapshotCollection));
                    writer.Serialize(file, snapshotList);
                    file.Flush();
                }
            }

            Console.WriteLine("Finished...");
            if (!batchMode)
                Console.ReadLine();
            return 0;
        }

        static void WriteInstructions()
        {
            Console.WriteLine(ParameterManager.GenerateCommandLineUsageMessage("MultiAgentConsole.exe"));

            Console.WriteLine("Press ENTER to exit...");

            Console.ReadLine();
        }
    }
}
