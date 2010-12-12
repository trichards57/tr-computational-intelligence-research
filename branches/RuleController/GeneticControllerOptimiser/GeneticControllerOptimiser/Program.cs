using System;
using System.Linq;
using GeneticControllerOptimiser.Classes;

/// @package GeneticControllerOptimiser
/// @brief Contains the <see cref="Program" /> class that runs the genetic controller optimiser.

namespace GeneticControllerOptimiser
{
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    /// <summary>
    /// The main class that runs when the program starts.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The main routine of the program. 
        /// </summary>
        /// <param name="args">The program's command line arguments.</param>
        /// <returns>
        /// An integer, where:
        /// - 0 - Success
        /// - 1 - Command Line Argument Error
        /// </returns>
        /// <remarks>
        /// This program runs all of the genetic algorithm.
        /// 
        /// First, the program processes the command line arguments, allowing the program 
        /// settings to be changed without recompiling. The algorithm then optimises the
        /// controller based on a set of fitness functions.  It divides the controller in to
        /// three segments, and processes them in this order
        /// 
        /// -# Angle Control
        /// -# Vertical Control
        /// -# Horizontal Control
        /// 
        /// This is done because good horizontal control first requires good angle and vertical
        /// control to be successful.  This also simplifies the fitness functions as only one
        /// main parameter needs to be checked at a time.  The final genome is output to the file
        /// genome.csv.
        /// </remarks>
        static int Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Genetic Algorithm Controller Optimisers : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");

            var genomeCount = 1000;
            var targetAngle = Math.PI / 4;
            var targetY = 20.0;
            var targetX = 20.0;
            var minusTargetY = -targetY;
            var minusTargetX = -targetX;
            var mutationRate = 0.75;
            var angleCycleCount = 500;
            var verticalCycleCount = 500;
            var horizontalCycleCount = 1000;
            var angleFitness = 1460;
            var verticalFitness = 1400;
            var horizontalFitness = 1860;
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
            var genomeBag = new ConcurrentBag<Genome>();

            Parallel.For(0, genomeCount, i =>
                {
                    var genome = new Genome();
                    for (var j = 0; j < 16; j++)
                        genome.Add(Genome.NewGene());
                    genomeBag.Add(genome);
                });

            var population = Population.Create(genomeBag);

            int fitness;

            Genome topGenome;

            PopulationMember.PastResults.Clear();

            do
            {
                var results = new ConcurrentBag<PopulationMember>();

                Parallel.ForEach(population, p => results.Add(p.Process(angleCycleCount, TargetVariables.Angle, 0, 0, Math.PI - targetAngle, Population.AngleFitnessCalculator, angleAccuracy)));

                population = Population.GetNew(results, mutationRate, Population.MutateAngle, out fitness, out topGenome);

                GC.Collect();

            } while (fitness < angleFitness);

            var bestAngleGenes = topGenome.Skip(13);

            genomeBag = new ConcurrentBag<Genome>();

            Parallel.For(0, genomeCount, i =>
            {
                var genome = new Genome();
                for (var j = 0; j < 13; j++)
                    genome.Add(Genome.NewGene());
                genome.AddRange(bestAngleGenes);
                genomeBag.Add(genome);
            });

            population = Population.Create(genomeBag, false, Math.PI);

            Console.WriteLine("Optimising vertical control...");

            PopulationMember.PastResults.Clear();

            do
            {
                var results = new ConcurrentBag<PopulationMember>();

                Parallel.ForEach(population, p => results.Add(p.Process(verticalCycleCount, TargetVariables.Vertical, 0, targetY, Math.PI, Population.VerticalFitnessCalculator, verticalAccuracy, true, 0, minusTargetY, Math.PI)));

                population = Population.GetNew(results, mutationRate, Population.MutateYControl, out fitness, out topGenome, p => (p.Fitness + p.NegativeFitness) / 2, false, Math.PI);

                GC.Collect();
            } while (fitness < verticalFitness);

            var bestYGenes = topGenome.Skip(6).Take(7);

            genomeBag = new ConcurrentBag<Genome>();

            Parallel.For(0, genomeCount, i =>
            {
                var genome = new Genome();
                for (var j = 0; j < 6; j++)
                    genome.Add(Genome.NewGene());
                genome.AddRange(bestYGenes);
                genome.AddRange(bestAngleGenes);
                genomeBag.Add(genome);
            });

            population = Population.Create(genomeBag, false, Math.PI);

            Console.WriteLine("Optimising horizontal control...");

            PopulationMember.PastResults.Clear();

            do
            {
                var results = new ConcurrentBag<PopulationMember>();

                Parallel.ForEach(population, p => results.Add(p.Process(horizontalCycleCount, TargetVariables.Horizontal | TargetVariables.Vertical, targetX, targetY, Math.PI, Population.HorizontalFitnessCalculator, horizontalAccuracy, true, minusTargetX, 0, Math.PI)));

                population = Population.GetNew(results, mutationRate, Population.MutateXControl, out fitness, out topGenome, p => (p.Fitness + p.NegativeFitness) / 2, false, Math.PI);

                GC.Collect();
            } while (fitness < horizontalFitness);

            Console.WriteLine("Writing genome data file.");
            using (var file = new StreamWriter(File.OpenWrite("genome.csv")))
            {
                foreach (var g in topGenome)
                    file.Write("{0},", g);
            }

            var finalController = Controller.FromGenome(topGenome);

            return 0;
        }

        /// <summary>
        /// Writes to the command line instructions on using the program.
        /// </summary>
        /// <remarks>
        /// This command pauses until the user hits enter.
        /// </remarks>
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
