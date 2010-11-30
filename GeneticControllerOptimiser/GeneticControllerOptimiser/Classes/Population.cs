using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticControllerOptimiser.Classes
{
    using global::System.Collections.Concurrent;
    using global::System.Threading.Tasks;

    static class Population
    {
        public static ParallelQuery<PopulationMember> GetNew(IEnumerable<PopulationMember> simulationResults, double mutationRate, Action<List<double>, double> mutateFunction, out int fitness, out IList<double> topGenome, Func<PopulationMember, int> fitnessTransformer = null, bool disableGravity = true, double initialAngle = 0)
        {
            if (fitnessTransformer == null)
                fitnessTransformer = new Func<PopulationMember, int>(a => a.Fitness);

            var orderedResults = simulationResults.OrderByDescending(fitnessTransformer);

            Console.WriteLine("Top Fitness     : {0}", orderedResults.Max(fitnessTransformer));
            Console.WriteLine("Average Fitness : {0}", orderedResults.Average(fitnessTransformer));

            var topThreeQuarterGenomes = orderedResults.Take(3 * simulationResults.Count() / 4).Select(p => p.Genome).ToList();
            var topQuarterGenomes = topThreeQuarterGenomes.Take(simulationResults.Count() / 4);

            var genomeBag = new ConcurrentBag<List<double>>();
            var mainPart = Partitioner.Create(topThreeQuarterGenomes);
            Parallel.ForEach(mainPart, genomeBag.Add);

            mainPart = Partitioner.Create(topQuarterGenomes);
            Parallel.ForEach(mainPart, g =>
            {
                var parent2 = topThreeQuarterGenomes[MultiRandom.Next(0, topThreeQuarterGenomes.Count)];
                genomeBag.Add(Breed(g, parent2));
            });

            Parallel.ForEach(genomeBag, g =>
            {
                if (!topQuarterGenomes.Contains(g) && MultiRandom.NextDouble() < mutationRate)
                    mutateFunction(g, mutationRate);
            });

            fitness = fitnessTransformer(orderedResults.First());
            topGenome = orderedResults.First().Genome;

            var population = Create(genomeBag);
            return population;
        }

        public static ParallelQuery<PopulationMember> Create(IEnumerable<List<double>> genomeList)
        {
            return genomeList.AsParallel().Select(g => new PopulationMember { Genome = g, Controller = Controller.FromGenome(g.ToArray()), System = new System(), NegativeController = Controller.FromGenome(g.ToArray()), NegativeSystem = new System() });
        }

        /// <summary>
        /// Breeds the specified <paramref name="item1"/> with <paramref name="item2"/>.
        /// </summary>
        /// <param name="item1">The first parent.</param>
        /// <param name="item2">The second parent.</param>
        /// <returns>The child genome.</returns>
        public static List<double> Breed(List<double> item1, List<double> item2)
        {
            var cutPoint = MultiRandom.Next(0, item1.Count);
            var child = item1.Take(cutPoint).ToList();
            child.AddRange(item2.Skip(cutPoint));

            return child;
        }

        public static double NewGene()
        {
            return MultiRandom.NextDouble() * 100 - 50;
        }

        public static void Mutate(IList<double> item, double mutationRate)
        {
            for (var i = 0; i < item.Count; i++)
            {
                if (MultiRandom.NextDouble() < mutationRate)
                    item[i] = MultiRandom.NextDouble() * 100 - 50;
            }
        }

        public static int HorizontalFitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            return GenericFitnessCalculator(results, ss => ss.X, targetX, accuracy);
        }

        public static int FitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            if (results.Any(s => s.Angle < (Math.PI / 2) || s.Angle > (3 * Math.PI / 2))) return 0;
            if (results.Any(s => s.Angle < (Math.PI / 4) || s.Angle > (5 * Math.PI / 4))) return 10;

            var horizontalFitness = GenericFitnessCalculator(results, ss => ss.X, targetX, accuracy);
            var verticalFitness = GenericFitnessCalculator(results, ss => ss.Y, targetY, accuracy);

            if (horizontalFitness <= 0 || verticalFitness <= 0) return 20;
            if (horizontalFitness <= 10 || verticalFitness <= 10) return 30;
            if (horizontalFitness <= 20 || verticalFitness <= 20) return 40;

            return (horizontalFitness + verticalFitness) / 2;
        }

        private static int GenericFitnessCalculator(IList<SystemState> results, Func<SystemState, double> dataTransform, double targetValue, double accuracy)
        {
            int fitness;

            var data = results.Select(dataTransform);

            data = targetValue < 0 ? data.Select(r => -r).ToList() : data;
            targetValue = Math.Abs(targetValue);

            var min = data.Min();
            var max = data.Max();


            if (min < -targetValue * accuracy)
            {
                // Must be unstable as it oscillates the wrong way.
                fitness = 0;
            }
            else if (max < targetValue * (1 - accuracy))
            {
                // Does not reach the target y (5% error allowed).
                fitness = 10;
            }
            else if (max > targetValue * 1.18)
            {
                // Overshoots too far.  Not very good. (18% error allowed.)
                fitness = 20;
            }
            else
            {
                var speed = 1500 - results.IndexOf(results.First(s =>
                {
                    var currentIndex = results.IndexOf(s);
                    return data.Skip(currentIndex + 1).All(r => r > targetValue * (1 - accuracy) && r < targetValue * (1 + accuracy));
                }));
                fitness = speed;
            }

            return fitness;
        }

        public static int VerticalFitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            return GenericFitnessCalculator(results, ss => ss.Y, targetY, accuracy);
        }
    }
}
