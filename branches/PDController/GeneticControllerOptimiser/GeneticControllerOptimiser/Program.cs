using System;
using System.Collections.Generic;
using System.Linq;
using GeneticControllerOptimiser.Classes;

namespace GeneticControllerOptimiser
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    class Program
    {
        static int Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Genetic Algorithm Controller Optimisers : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");

            var genomeCount = 1000;
            var targetY = 20.0;
            var targetX = 20.0;
            var minusTargetY = -targetY;
            var minusTargetX = -targetX;
            var mutationRate = 0.5;
            var cycleCount = 5000;
            var targetFitness = 860;
            var accuracy = 0.01;

            var processedArguments = args.Select(item =>
            {
                var parts = item.Split(new[] { ":" }, StringSplitOptions.None);

                var integer = 0;
                var doub = 0.0;
                if (parts.Length == 2)
                {
                    var success = int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.CurrentCulture, out integer);
                    if (!success)
                        integer = 0;
                    success = double.TryParse(parts[1], NumberStyles.Float, CultureInfo.CurrentCulture, out doub);
                    if (!success)
                        doub = 0;
                }
                return new { ParamLabel = parts[0], Value = integer, DoubleValue = doub };
            });
            foreach (var a in processedArguments)
            {
                switch (a.ParamLabel)
                {
                    case "/g":
                        genomeCount = a.Value;
                        break;
                    case "/tx":
                        targetX = a.DoubleValue;
                        minusTargetX = -targetX;
                        break;
                    case "/ty":
                        targetY = a.DoubleValue;
                        minusTargetY = -targetY;
                        break;
                    case "/mr":
                        mutationRate = a.DoubleValue;
                        break;
                    default:
                        Console.WriteLine("Unknown Parameter : {0}", a.ParamLabel);
                        WriteInstructions();
                        return 1;
                }
            }

            Console.WriteLine("Genome Count           : {0}", genomeCount);
            Console.WriteLine("Target X               : {0}", targetX);
            Console.WriteLine("Target Y               : {0}", targetY);
            Console.WriteLine("Mutation Rate          : {0}", mutationRate);
            Console.WriteLine("Cycle Count            : {0}", cycleCount);
            Console.WriteLine();

            Console.WriteLine("Controller...");
            var genomeBag = new ConcurrentBag<List<double>>();

            Parallel.For(0, genomeCount, i =>
                {
                    var genome = new List<double>();
                    for (var j = 0; j < 8; j++)
                        genome.Add(Population.NewGene());
                    genomeBag.Add(genome);
                });

            var population = Population.Create(genomeBag);

            int fitness;

            IList<double> topGenome;

            do
            {
                var results = new ConcurrentBag<PopulationMember>();

                Parallel.ForEach(population, p => results.Add(p.Process(cycleCount, targetX, targetY, Math.PI, Population.FitnessCalculator, accuracy)));
                population = Population.GetNew(results, mutationRate, Population.Mutate, out fitness, out topGenome);

                GC.Collect();

            } while (!Console.KeyAvailable);

            Console.WriteLine("Writing genome data file.");
            using (var file = new StreamWriter(File.OpenWrite("genome.csv")))
            {
                foreach (var g in topGenome)
                    file.Write("{0},", g);
            }

            return 0;
        }

        private static void WriteInstructions()
        {
            Console.WriteLine("Command Line Usage : ");
            Console.WriteLine("GAOptimiser.exe");
            Console.WriteLine("    /g   : An integer specifying the number of genomes in a population.");
            Console.WriteLine("           Default : 1000");
            Console.WriteLine("    /ta  : A double specifying the angle to use for angle control");
            Console.WriteLine("           testing. Default : PI/4");
            Console.WriteLine("    /tx  : A double specifying the x coordinate to use for horizontal control");
            Console.WriteLine("           testing. Default : 20");
            Console.WriteLine("    /ty  : A double specifying the y coordinate to use for horizontal control");
            Console.WriteLine("           testing. Default : 20");
            Console.WriteLine("    /mr  : A double specifying the mutation rate of the alrogithm. Must be less");
            Console.WriteLine("           than 1.");
            Console.WriteLine("    /acc : An integer specifying the number of cycles to test the angle control");
            Console.WriteLine("           for. Default : 500");
            Console.WriteLine("    /hcc : An integer specifying the number of cycles to test the horizontals");
            Console.WriteLine("           control for. Default : 500");
            Console.WriteLine("    /vcc : An integer specifying the number of cycles to test the vertical");
            Console.WriteLine("           control for. Default : 500");
            Console.WriteLine("    /af  : An integer specifying the target fitness for the angle controller.");
            Console.WriteLine("           Default : 960");
            Console.WriteLine("    /vf  : An integer specifying the target fitness for the vertical controller.");
            Console.WriteLine("           Default : 900");
            Console.WriteLine("    /hf  : An integer specifying the target fitness for the horizontal controller.");
            Console.WriteLine("           Default : 860");
            Console.WriteLine("    /aa  : An integer specifying the target accuracy for the angle controller.");
            Console.WriteLine("           Default : 0.01");
            Console.WriteLine("    /va  : An integer specifying the target accuracy for the vertical controller.");
            Console.WriteLine("           Default : 0.05");
            Console.WriteLine("    /ha  : An integer specifying the target accuracy for the horizontal controller.");
            Console.WriteLine("           Default : 0.1");
            Console.WriteLine();
            Console.WriteLine("Return Values : ");
            Console.WriteLine("0 : Success");
            Console.WriteLine("1 : Argument Error");
            Console.ReadLine();
        }
    }
}
