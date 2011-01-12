﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using MultiAgentLibrary;

/// @package MultiAgentConsole
/// @brief Contains the Multi-Agent Route Finder program

namespace MultiAgentConsole
{
    /// <summary>
    /// The main class that runs when the program starts.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The main routine of the program
        /// </summary>
        /// <param name="args">The program's command line arguments.</param>
        /// <returns>An integer, where:
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 - Success</description>
        ///         </item>
        ///         <item>
        ///             <description>1 - Command Line Argument Error</description>
        ///         </item>
        ///         <item>
        ///             <description>2 - Data File Not Found</description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <remarks>
        /// This function runs all of the multi-agent processing.
        /// 
        /// First, the function processes the command line arguments, allowing the program settings
        /// to be changed without recompiling.  It then loads the specified data file in to a 
        /// <see cref="Field"/>, intialises the field with the number of starting agents, and then
        /// runs the simulation for the specified number of cycles.  If the program is compiled with
        /// the MOVIE preprocessor variable set, it will also output a bitmap every ten cycles to
        /// show what is happening.
        /// 
        /// When the system is finished, it outputs a bitmap to output.gif, and the shortest route
        /// that was found is output to route.csv.  This route is processed to remove any points
        /// that do not add to the route (i.e. the route is expressed with the minimum number of points).
        /// </remarks>
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

            var dataFile = args[0];
            var mapWidth = 100;
            var mapHeight = 100;
            var maxAgents = 250;
            var startAgents = 1;
            var cycleCount = 40000;
            var memoryLength = 4;

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Data file {0} does not exist.", args[0]);
                return 2;
            }

            if (args.Length > 1)
            {
                var extraArgs = args.Where((item, index) => index > 0).Select(item =>
                    {
                        var parts = item.Split(new[] { ":" }, StringSplitOptions.None);
                        return new { ParamLabel = parts[0], Value = int.Parse(parts[1]) };
                    });
                foreach (var a in extraArgs)
                {
                    switch (a.ParamLabel)
                    {
                        case "/w":
                            mapWidth = a.Value;
                            break;
                        case "/h":
                            mapHeight = a.Value;
                            break;
                        case "/ma":
                            maxAgents = a.Value;
                            break;
                        case "/sa":
                            startAgents = a.Value;
                            break;
                        case "/c":
                            cycleCount = a.Value;
                            break;
                        case "/sm":
                            memoryLength = a.Value;
                            break;
                        default:
                            Console.WriteLine("Unknown Parameter : {0}", a.ParamLabel);
                            WriteInstructions();
                            return 1;
                    }
                }
            }

            Console.WriteLine("Data File                : {0}", dataFile);
            Console.WriteLine("Map Width                : {0}", mapWidth);
            Console.WriteLine("Map Height               : {0}", mapHeight);
            Console.WriteLine("Max Agents               : {0}", maxAgents);
            Console.WriteLine("Starting Agents          : {0}", startAgents);
            Console.WriteLine("Cycle Count              : {0}", cycleCount);
            Console.WriteLine("Short Term Memory Length : {0}", memoryLength);
            Console.WriteLine();

            Console.WriteLine("Loading data file...");
            var field = new Field(mapWidth, mapHeight, dataFile);
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
#if MOVIE
                if (i % 10 == 0)
                {
                var oImage = new System.Drawing.Bitmap(mapWidth * 10, mapHeight * 10);
                using (var graphics = System.Drawing.Graphics.FromImage(oImage))
                {
                    foreach (var square in field.Squares)
                    {
                        var col = square.SquareColor;
                        graphics.FillRectangle(new SolidBrush(col), (int)square.Position.X * 10, (int)square.Position.Y * 10, 10, 10);
                    }
                    foreach (var agent in field.AgentsList)
                    {
                        graphics.FillEllipse(Brushes.Yellow, (int)agent.Position.X * 10, (int)agent.Position.Y * 10, 10, 10);
                    }
                }

                oImage.Save(string.Format(@".\Frames\output{0:00000000}.gif", i), ImageFormat.Gif);
                oImage.Dispose();
                }
#endif
            }
            stopwatch.Stop();
            Console.WriteLine("Simulation stopped.  Total Time : {0} seconds", stopwatch.Elapsed.TotalSeconds);
            Console.WriteLine();

            Console.WriteLine("Outputing final state image.");

            var outputImage = new Bitmap(mapWidth * 10, mapHeight * 10);
            using (var graphics = Graphics.FromImage(outputImage))
            {

                foreach (var square in field.Squares)
                {
                    var col = square.SquareColour;
                    graphics.FillRectangle(new SolidBrush(col), square.Position.X * 10, square.Position.Y * 10, 10, 10);
                }
                foreach (var point in field.ShortestRoute)
                {
                    graphics.FillEllipse(Brushes.Yellow, point.X * 10 + 2.5f, point.Y * 10 + 2.5f, 5.0f, 5.0f);
                }
            }

            outputImage.Save("output.gif", ImageFormat.Gif);

            Console.WriteLine("Image written.");

            Console.WriteLine("Removing uneeded route points");
            var scaledData = field.ShortestRoute.Select(p => new PointF(p.X * field.SquareSize.Width, p.Y * field.SquareSize.Height)).ToList();

            var result = new List<PointF>();

            for (var i = 0; i < scaledData.Count(); i++)
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
            using (var file = new StreamWriter(File.OpenWrite("route.csv")))
            {
                foreach (var p in result)
                {
                    file.WriteLine("{0},{1}", p.X, p.Y);
                }
            }
            Console.WriteLine("Finished...");

            Console.ReadLine();
            return 0;
        }

        /// <summary>
        /// Writes instructions to the command line which explain all of the command line switches available
        /// to modifiy the data.  It then pauses at the end of the output until the user presses enter.
        /// </summary>
        static void WriteInstructions()
        {
            Console.WriteLine("Command Line Usage : ");
            Console.WriteLine("MultiAgentConsole.exe DataFile.csv [/w:width] [/h:height] [/ma:maxAgentCount]");
            Console.WriteLine("                                   [/sa:startingAgentCount] [/c:CycleCount]");
            Console.WriteLine("    /w  : An integer specifying the desired width of the map. Default : 100");
            Console.WriteLine("    /h  : An integer specifying the desired height of the map.");
            Console.WriteLine("          Default : 100");
            Console.WriteLine("    /ma : An integer specifying the maximum number of agents for the");
            Console.WriteLine("          simulation. Default : 250");
            Console.WriteLine("    /sa : An integer specifying the starting number of agents for the");
            Console.WriteLine("          simulation. Default : 1");
            Console.WriteLine("    /c  : An integer specifying the number of cycles the sequence will");
            Console.WriteLine("          run for. Default : 40000");
            Console.WriteLine("    /sm : An integer specifying the length of the agent's short term route");
            Console.WriteLine("          memory, which is used to prevent back tracking. Default : 4");
            Console.WriteLine();
            Console.WriteLine("Return Values : ");
            Console.WriteLine("0 : Success");
            Console.WriteLine("1 : Argument Error");
            Console.WriteLine("2 : Data File Not Found");
            Console.ReadLine();
        }
    }
}
