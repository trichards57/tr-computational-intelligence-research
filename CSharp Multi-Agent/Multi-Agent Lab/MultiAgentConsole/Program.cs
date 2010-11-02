﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiAgentLibrary;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;

namespace MultiAgentConsole
{
    class Program
    {
        static int Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Multi-Agent Lab Console : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");
            if (args.Length < 1|| !File.Exists(args[0]))
            {
                WriteInstructions();
                return 1;
            }

            var dataFile = args[0];
            var mapWidth = 100;
            var mapHeight = 100;
            var maxAgents = 250;
            var startAgents = 1;
            var cycleCount = 10000;

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
                        default:
                            Console.WriteLine("Unknown Paramater : {0}", a.ParamLabel);
                            WriteInstructions();
                            return 1;
                    }
                }
            }

            Console.WriteLine("Data File       : {0}", dataFile);
            Console.WriteLine("Map Width       : {0}", mapWidth);
            Console.WriteLine("Map Height      : {0}", mapHeight);
            Console.WriteLine("Max Agents      : {0}", maxAgents);
            Console.WriteLine("Starting Agents : {0}", startAgents);
            Console.WriteLine("Cycle Count     : {0}", cycleCount);
            Console.WriteLine();

            Console.WriteLine("Loading data file...");
            var field = new Field(mapWidth, mapHeight, dataFile);
            Console.WriteLine("Data file loaded.");
            Console.WriteLine();

            Console.WriteLine("Setting up initial agents...");
            for (var i = 0; i < startAgents; i++)
                field.AgentsList.Add(new Agent(field.StartPoint));
            Console.WriteLine("Initial agents set up.");
            Console.WriteLine();

            Console.WriteLine("Starting simulation.");

            for (var i = 0; i < cycleCount; i++)
            {
                if ((i + 1) % 10 == 0 && field.AgentsList.Count < maxAgents)
                {
                    field.AgentsList.Add(new Agent(field.StartPoint));
                    Console.WriteLine("Adding agent...");
                }
                field.CycleAgents();
                Console.WriteLine("Cycle : {0}", i);
            }

            Console.WriteLine("Simulation stopped.");
            Console.WriteLine("");

            Console.WriteLine("Outputing final state image.");

            var outputImage = new System.Drawing.Bitmap(mapWidth * 10, mapHeight * 10);
            using (var graphics = System.Drawing.Graphics.FromImage(outputImage))
            {
                foreach (var square in field.SelectMany(r => r))
                {
                    var col = System.Drawing.Color.FromArgb(square.SquareColor.A, square.SquareColor.R, square.SquareColor.G, square.SquareColor.B);
                    graphics.FillRectangle(new SolidBrush(col), (int)square.Position.X * 10, (int)square.Position.Y * 10, 10, 10);
                }
                foreach (var agent in field.AgentsList)
                {
                    graphics.FillEllipse(Brushes.Yellow, (int)agent.Position.X * 10, (int)agent.Position.Y * 10, 10, 10);
                }
            }

            outputImage.Save("output.bmp", ImageFormat.Bmp);

            Console.WriteLine("Image written.");
            Console.WriteLine("Finished...");

            Console.ReadLine();
            return 0;
        }

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
            Console.WriteLine("          run for. Default : 10000");
            Console.WriteLine();
            Console.WriteLine("Return Values : ");
            Console.WriteLine("0 : Success");
            Console.WriteLine("1 : Argument Error");
            Console.WriteLine("2 : Data File Not Found");
            Console.ReadLine();
        }
    }
}