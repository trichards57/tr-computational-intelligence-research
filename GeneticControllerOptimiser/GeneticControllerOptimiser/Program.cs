using System;
using System.Collections.Generic;
using System.Linq;
using GeneticControllerOptimiser.Classes;

namespace GeneticControllerOptimiser
{
    class Program
    {
        const int GenomeCount = 100;
        const double MutationRate = 0.5;
        static readonly Random Rand = new Random();

        static void Main()
        {
            var testGenome = new[] { 20, 5, 2.5, 20, 5, 2.5, 50, 20, 50, 20, 0.3, 0.1, 0.1, 6, 5, 0 };

            var testController = Controller.FromGenome(testGenome);

            var list = new List<List<double>>(GenomeCount);

            for (var i = 0; i < GenomeCount; i++)
            {
                list.Add(new List<double>(16));
                for (var j = 0; j < 16; j++)
                {
                    list[i].Add(Rand.NextDouble() * 100 - 50);
                }
            }

            while (!Console.KeyAvailable)
            {
                var population = list.Select(g => new { Genome = g, Controller = Controller.FromGenome(g.ToArray()), System = new Classes.System { DisableGravity = true } });

                var results = population.Select(p =>
                    {
                        var systemState = new SystemState();
                        var result = new List<SystemState> {systemState};
                        for (var i = 0; i < 1000; i++)
                        {
                            var thrusterState = p.Controller.Process(systemState, 0, 0, Math.PI - Math.PI / 4);
                            systemState = p.System.Process(thrusterState);
                            result.Add(systemState);
                        }
                        return new { ResultData = result, p.Controller, p.System, Fitness = FitnessCalculator(result, 0, 0, Math.PI / 4), p.Genome };
                    }).OrderByDescending(r => r.Fitness).ToList();

                var bestController = results.First().ResultData.Select(s => s.Angle);
                var bestControllerCsv = string.Join(",", bestController.Select(a => a.ToString()).ToArray());

                Console.WriteLine("Top Fitness     : {0}", results.Max(a => a.Fitness));
                Console.WriteLine("Average Fitness : {0}", results.Average(a => a.Fitness));

                var topThreeQuarterGenomes = results.Take(3 * GenomeCount / 4).Select(p => p.Genome).ToList();
                var topQuarterGenomes = results.Take(GenomeCount / 4).Select(p => p.Genome).ToList();

                list = topThreeQuarterGenomes;

                for (var i = 0; i < GenomeCount / 4; i++)
                {
                    var parent1 = topQuarterGenomes[i];
                    var parent2 = topThreeQuarterGenomes[Rand.Next(0, topThreeQuarterGenomes.Count)];
                    list.Add(Breed(parent1, parent2));
                }
                for (var i = GenomeCount / 4; i < GenomeCount; i++)
                {
                    if (Rand.NextDouble() < MutationRate)
                        list[i] = MutateAngle(list[i]);
                }
            }

        }

        /// <summary>
        /// Calculates the fitness for the given system results.
        /// </summary>
        /// <param name="results">The results to analyse.</param>
        /// <param name="targetX">The target X coordinate.</param>
        /// <param name="targetY">The target Y coordinate.</param>
        /// <param name="targetAngle">The target angle.</param>
        /// <returns></returns>
        public static int FitnessCalculator(List<SystemState> results, double targetX, double targetY, double targetAngle)
        {
            int fitness;

            if (Math.Sign(results[2].Angle) != Math.Sign(targetAngle))
            {
                // Immediatly started heading the wrong way.  No good at all.
                fitness = 0;
            }
            else if (results.Min(s => s.Angle) < 0)
            {
                // Must be unstable as it oscillates the wrong way.
                fitness = 0;
            }
            else if (results.Max(s => s.Angle) > targetAngle * 1.18)
            {
                // Overshoots too far.  Not very good. (18% error allowed.)
                fitness = 10;
            }
            else if (results.Max(s => s.Angle) < targetAngle * 0.99)
            {
                // Does not reach the target angle (1% error allowed).
                fitness = 20;
            }
            else
            {
                var speed = results.Count - results.IndexOf(results.First(s =>
                {
                    var currentIndex = results.IndexOf(s);
                    return results.Skip(currentIndex + 1).All(r => r.Angle > targetAngle * 0.9 && r.Angle < targetAngle * 1.1);
                }));
                fitness = speed;
            }

            return fitness;
        }

        /// <summary>
        /// Breeds the specified <paramref name="item1"/> with <paramref name="item2"/>.
        /// </summary>
        /// <param name="item1">The first parent.</param>
        /// <param name="item2">The second parent.</param>
        /// <returns>The child genome.</returns>
        public static List<double> Breed(List<double> item1, List<double> item2)
        {
            var cutPoint = Rand.Next(0, item1.Count);
            var child = item1.Take(cutPoint).ToList();
            child.AddRange(item2.Skip(cutPoint));

            return child;
        }

        /// <summary>
        /// Mutates the genes related to angle control in <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to mutate.</param>
        /// <returns>The mutated item.</returns>
        public static List<double> MutateAngle(List<double> item)
        {
            var index = Rand.Next(13, item.Count);

            item[index] = Rand.NextDouble() * 100 - 50;

            return item;
        }
    }
}
