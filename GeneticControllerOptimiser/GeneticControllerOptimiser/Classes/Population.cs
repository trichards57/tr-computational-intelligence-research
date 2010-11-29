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

            var population = Create(genomeBag, disableGravity, initialAngle);
            return population;
        }

        public static ParallelQuery<PopulationMember> Create(IEnumerable<List<double>> genomeList, bool disableGravity = true, double initialAngle = 0.0)
        {
            return genomeList.AsParallel().Select(g => new PopulationMember { Genome = g, Controller = Controller.FromGenome(g.ToArray()), System = new System { DisableGravity = disableGravity, Angle = initialAngle }, NegativeController = Controller.FromGenome(g.ToArray()), NegativeSystem = new System { DisableGravity = disableGravity, Angle = initialAngle } });
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

        /// <summary>
        /// Mutates the genes related to angle control in <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to mutate.</param>
        /// <returns>The mutated item.</returns>
        public static void MutateAngle(List<double> item, double mutationRate)
        {
            var index = MultiRandom.Next(13, item.Count);

            item[index] = MultiRandom.NextDouble() * 100 - 50;
        }

        public static void MutateYControl(List<double> item, double mutationRate)
        {
            for (var i = 6; i < 13; i++)
            {
                if (MultiRandom.NextDouble() < mutationRate)
                    item[i] = MultiRandom.NextDouble() * 100 - 50;
            }
        }

        public static void MutateXControl(IList<double> item, double mutationRate)
        {
            for (var i = 0; i < 6; i++)
            {
                if (MultiRandom.NextDouble() < mutationRate)
                    item[i] = MultiRandom.NextDouble() * 100 - 50;
            }
        }

        public static int HorizontalFitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            return GenericFitnessCalculator(results, ss => ss.X, targetX, accuracy);
        }

        /// <summary>
        /// Calculates the fitness for the given system results.
        /// </summary>
        /// <param name="results">The results to analyse.</param>
        /// <param name="targetAngle">The target angle.</param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public static int AngleFitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            return GenericFitnessCalculator(results, ss => ss.Angle, targetAngle, accuracy);
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
                var speed = 1000 - results.IndexOf(results.First(s =>
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
