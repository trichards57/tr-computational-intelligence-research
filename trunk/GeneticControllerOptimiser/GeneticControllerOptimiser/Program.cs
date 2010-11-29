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
            var targetAngle = Math.PI / 4;
            var targetY = 100.0;
            var targetX = 50.0;
            var minusTargetY = -targetY;
            var minusTargetX = -targetX;
            var mutationRate = 0.75;
            var angleCycleCount = 500;
            var verticalCycleCount = 2500;
            var horizontalCycleCount = 2500;
            var angleFitness = 4960;
            var verticalFitness = 2500;
            var horizontalFitness = 2500;
            var angleAccuracy = 0.01;
            var verticalAccuracy = 0.05;
            var horizontalAccuracy = 0.1;

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
                    case "/ta":
                        targetAngle = a.DoubleValue;
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
                    case "/acc":
                        angleCycleCount = a.Value;
                        break;
                    case "/hcc":
                        horizontalCycleCount = a.Value;
                        break;
                    case "/vcc":
                        verticalCycleCount = a.Value;
                        break;
                    case "/af":
                        angleFitness = a.Value;
                        break;
                    case "/vf":
                        verticalFitness = a.Value;
                        break;
                    case "/hf":
                        horizontalFitness = a.Value;
                        break;
                    case "/aa":
                        angleAccuracy = a.DoubleValue;
                        break;
                    case "/va":
                        verticalAccuracy = a.DoubleValue;
                        break;
                    case "/ha":
                        horizontalAccuracy = a.DoubleValue;
                        break;
                    default:
                        Console.WriteLine("Unknown Parameter : {0}", a.ParamLabel);
                        WriteInstructions();
                        return 1;
                }
            }

            Console.WriteLine("Genome Count           : {0}", genomeCount);
            Console.WriteLine("Target Angle           : {0}", targetAngle);
            Console.WriteLine("Target X               : {0}", targetX);
            Console.WriteLine("Target Y               : {0}", targetY);
            Console.WriteLine("Mutation Rate          : {0}", mutationRate);
            Console.WriteLine("Angle Cycle Count      : {0}", angleCycleCount);
            Console.WriteLine("Horizontal Cycle Count : {0}", horizontalCycleCount);
            Console.WriteLine("Vertical Cycle Count   : {0}", verticalCycleCount);
            Console.WriteLine("Angle Accuracy         : {0}", angleAccuracy);
            Console.WriteLine("Vertical Accuracy      : {0}", verticalAccuracy);
            Console.WriteLine("Horizontal Accuracy    : {0}", horizontalAccuracy);
            Console.WriteLine("Angle Fitness          : {0}", angleFitness);
            Console.WriteLine("Vertical Fitness       : {0}", verticalFitness);
            Console.WriteLine("Horizontal Fitness     : {0}", horizontalFitness);
            Console.WriteLine();

            Console.WriteLine("Optimising angle control...");
            var genomeBag = new ConcurrentBag<List<double>>();

            Parallel.For(0, genomeCount, i =>
                {
                    var genome = new List<double>();
                    for (var j = 0; j < 16; j++)
                        genome.Add(Population.NewGene());
                    genomeBag.Add(genome);
                });

            var population = Population.Create(genomeBag);

            int fitness;

            IList<double> topGenome;

            do
            {
                var results = new ConcurrentBag<PopulationMember>();

                Parallel.ForEach(population, p => results.Add(p.Process(angleCycleCount, 0, 0, Math.PI - targetAngle, Population.AngleFitnessCalculator, angleAccuracy)));

                population = Population.GetNew(results, mutationRate, Population.MutateAngle, out fitness, out topGenome);

                GC.Collect();

            } while (fitness < angleFitness);

            var bestAngleGenes = topGenome.Skip(13);

            genomeBag = new ConcurrentBag<List<double>>();

            Parallel.For(0, genomeCount, i =>
            {
                var genome = new List<double>();
                for (var j = 0; j < 13; j++)
                    genome.Add(Population.NewGene());
                genome.AddRange(bestAngleGenes);
                genomeBag.Add(genome);
            });

            population = Population.Create(genomeBag, false, Math.PI);

            Console.WriteLine("Optimising vertical control...");

            do
            {
                var results = new ConcurrentBag<PopulationMember>();

                Parallel.ForEach(population, p => results.Add(p.Process(verticalCycleCount, 0, targetY, Math.PI, Population.VerticalFitnessCalculator, verticalAccuracy, true, 0, minusTargetY, Math.PI)));

                population = Population.GetNew(results, mutationRate, Population.MutateYControl, out fitness, out topGenome, p => (p.Fitness + p.NegativeFitness) / 2, false, Math.PI);

                GC.Collect();
            } while (fitness < verticalFitness);

            var bestYGenes = topGenome.Skip(6).Take(7);

            genomeBag = new ConcurrentBag<List<double>>();

            Parallel.For(0, genomeCount, i =>
            {
                var genome = new List<double>();
                for (var j = 0; j < 6; j++)
                    genome.Add(Population.NewGene());
                genome.AddRange(bestYGenes);
                genome.AddRange(bestAngleGenes);
                genomeBag.Add(genome);
            });

            population = Population.Create(genomeBag, false, Math.PI);

            Console.WriteLine("Optimising horizontal control...");

            do
            {
                var results = new ConcurrentBag<PopulationMember>();

                Parallel.ForEach(population, p => results.Add(p.Process(horizontalCycleCount, targetX, 0, Math.PI, Population.HorizontalFitnessCalculator, horizontalAccuracy, true, minusTargetX, 0, Math.PI)));

                population = Population.GetNew(results, mutationRate, Population.MutateXControl, out fitness, out topGenome, p => (p.Fitness + p.NegativeFitness) / 2, false, Math.PI);

                GC.Collect();
            } while (fitness < horizontalFitness);

            Console.WriteLine("Writing genome data file.");
            using (var file = new StreamWriter(File.OpenWrite("genome.csv")))
            {
                foreach (var g in topGenome)
                    file.Write("{0},", g);
            }

            var finalC = Controller.FromGenome(topGenome.ToArray());

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
