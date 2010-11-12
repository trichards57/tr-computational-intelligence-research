using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace RouteOptimiser
{
    class Program
    {
        static int Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Multi-Agent Route Optimiser : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");
            if (args.Length < 1)
            {
                WriteInstructions();
                return 1;
            }

            var dataFile = args[0];

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Data file {0} does not exist.", args[0]);
                return 2;
            }

            var file = File.ReadAllLines(dataFile);

            var points = file.AsParallel().Select(l => {
                var parts = l.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                return new PointF(float.Parse(parts[0]), float.Parse(parts[1]));
            }).ToList();

            var result = new List<PointF>();

            for (var i = 0; i < points.Count; i++)
            {
                if (result.Count < 2)
                    result.Add(points[i]);
                else
                {
                    var p1 = result[result.Count - 2];
                    var p2 = result[result.Count - 1];

                    var m = (p1.Y - p2.Y) / (p1.X - p2.X);
                    var c = p1.Y - m * p1.X;

                    if (float.IsInfinity(m) && points[i].X == p1.X)
                        result[result.Count - 1] = points[i];
                    else if (m == 0 && points[i].Y == p1.Y)
                        result[result.Count - 1] = points[i];
                    else if (m * points[i].X + c == points[i].Y)
                        result[result.Count - 1] = points[i];
                    else
                        result.Add(points[i]);
                }
            }

            using (var outFile = new StreamWriter(File.OpenWrite("routeNew.csv")))
            {
                foreach (var p in result)
                {
                    outFile.WriteLine("{0},{1}", p.X, p.Y);
                }
            }

                return 0;
        }

        static void WriteInstructions()
        {
            Console.WriteLine("Command Line Usage : ");
            Console.WriteLine("RouteOptimiser.exe DataFile.csv");
            Console.WriteLine();
            Console.WriteLine("Return Values : ");
            Console.WriteLine("0 : Success");
            Console.WriteLine("1 : Argument Error");
            Console.WriteLine("2 : Data File Not Found");
            Console.ReadLine();
        }
    }
}
