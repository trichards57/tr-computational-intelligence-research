using System;
using System.Collections.Generic;
using System.Linq;

namespace GeneticControllerOptimiser.Classes
{
    using global::System.Collections.Concurrent;
    using global::System.Threading.Tasks;

    /// <summary>
    /// A static class that contains all the functions used to manage populations.
    /// </summary>
    static class Population
    {
        /// <summary>
        /// Creates the next generation of a population.
        /// </summary>
        /// <param name="simulationResults">The results of the last simulation.</param>
        /// <param name="mutationRate">The mutation rate.</param>
        /// <param name="mutateFunction">The mutate function.</param>
        /// <param name="fitness">The best fitness of the previous population.</param>
        /// <param name="topGenome">The best genome of the previous population.</param>
        /// <param name="fitnessTransformer">A function to transform the population member's fitness values in to a single number.</param>
        /// <param name="disableGravity">if set to <c>true</c> disable gravity in the simulation.</param>
        /// <param name="initialAngle">The initial angle of each pod.</param>
        /// <returns>An IEnumerable&lt;<see cref="PopulationMember"/>&gt; containing the new population.</returns>
        /// <remarks>
        /// This function first sorts the old population based on the fitness calcilated using 
        /// <paramref name="fitnessTransformer"/>. The bottom quarter are discarded, and replaced by breeding each member 
        /// of the top quarter of the population with a random member of the top three quarters.  The bottom
        /// three quarters are then mutated, using <paramRef name="mutationRate"/> and <paramref name="mutateFunction"/>.
        /// The result is then returned.  The order of the returned population is not specified.
        /// </remarks>
        public static IEnumerable<PopulationMember> GetNew(IEnumerable<PopulationMember> simulationResults, double mutationRate, Action<Genome, double> mutateFunction, out int fitness, out Genome topGenome, Func<PopulationMember, int> fitnessTransformer = null, bool disableGravity = true, double initialAngle = 0)
        {
            if (fitnessTransformer == null)
                fitnessTransformer = new Func<PopulationMember, int>(a => a.Fitness);

            var orderedResults = simulationResults.OrderByDescending(fitnessTransformer);

            Console.WriteLine("Top Fitness     : {0}", orderedResults.Max(fitnessTransformer));
            Console.WriteLine("Average Fitness : {0}", orderedResults.Average(fitnessTransformer));

            var topThreeQuarterGenomes = orderedResults.Take(3 * simulationResults.Count() / 4).Select(p => p.Genome).ToList();
            var topQuarterGenomes = topThreeQuarterGenomes.Take(simulationResults.Count() / 4);

            var genomeBag = new ConcurrentBag<Genome>();
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

        /// <summary>
        /// Transforms a list of genomes to a list of population members.
        /// </summary>
        /// <param name="genomeList">The genome list to transform.</param>
        /// <param name="disableGravity">if set to <c>true</c>, disable the gravity in the simulator.</param>
        /// <param name="initialAngle">The initial angle of each pod.</param>
        /// <returns>A list of <see cref="PopulationMember"/> which are ready to be tested.</returns>
        public static IEnumerable<PopulationMember> Create(IEnumerable<Genome> genomeList, bool disableGravity = true, double initialAngle = 0.0)
        {
            return genomeList.AsParallel().Select(g => new PopulationMember { Genome = g, Controller = Controller.FromGenome(g), System = new System { DisableGravity = disableGravity, Angle = initialAngle }, NegativeController = Controller.FromGenome(g), NegativeSystem = new System { DisableGravity = disableGravity, Angle = initialAngle } });
        }

        /// <summary>
        /// Breeds the specified <paramref name="item1"/> with <paramref name="item2"/>.
        /// </summary>
        /// <param name="item1">The first parent.</param>
        /// <param name="item2">The second parent.</param>
        /// <returns>The child genome.</returns>
        public static Genome Breed(Genome item1, Genome item2)
        {
            var cutPoint = MultiRandom.Next(0, item1.Count);
            var g = new Genome();
            g.AddRange(item1.Take(cutPoint).ToList());
            g.AddRange(item2.Skip(cutPoint));

            return g;
        }


        /// <summary>
        /// Mutates the genes related to angle control in <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to mutate.</param>
        /// <param name="mutationRate">The chance that an item will be mutated (unused)</param>
        public static void MutateAngle(Genome item, double mutationRate)
        {
            var index = MultiRandom.Next(13, 16);

            item[index] = MultiRandom.NextDouble() * 100 - 50;
        }

        /// <summary>
        /// Mutates the genes related to Y control in <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to mutate.</param>
        /// <param name="mutationRate">The chance that an item will be mutated (unused)</param>
        public static void MutateYControl(Genome item, double mutationRate)
        {
            var index = MultiRandom.Next(6, 11);

            item[index] = MultiRandom.NextDouble() * 100 - 50;
        }

        /// <summary>
        /// Mutates the genes related to X control in <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to mutate.</param>
        /// <param name="mutationRate">The chance that an item will be mutated (unused)</param>
        public static void MutateXControl(Genome item, double mutationRate)
        {
            var index = MultiRandom.Next(0, 5);

            item[index] = MultiRandom.NextDouble() * 100 - 50;
        }

        /// <summary>
        /// Calculates the fitness of a genome when performing horizontal movement.
        /// </summary>
        /// <param name="results">The system results to analyse.</param>
        /// <param name="targetX">The target X coordinate.</param>
        /// <param name="targetY">The target Y coordinate (unused).</param>
        /// <param name="targetAngle">The target angle (unused).</param>
        /// <param name="accuracy">The required accuracy.</param>
        /// <returns>An integer representing the fitness of the genome.  Higher numbers are better.</returns>
        public static int HorizontalFitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            return GenericFitnessCalculator(results, ss => ss.X, targetX, accuracy);
        }

        /// <summary>
        /// Calculates the fitness of a genome when performing rotational movement.
        /// </summary>
        /// <param name="results">The system results to analyse.</param>
        /// <param name="targetX">The target X coordinate (unused).</param>
        /// <param name="targetY">The target Y coordinate (unused).</param>
        /// <param name="targetAngle">The target angle.</param>
        /// <param name="accuracy">The required accuracy accuracy.</param>
        /// <returns>An integer representing the fitness of the genome.  Higher numbers are better.</returns>
        public static int AngleFitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            return GenericFitnessCalculator(results, ss => ss.Angle, targetAngle, accuracy);
        }

        /// <summary>
        /// Calculates the fitness of a set of results.
        /// </summary>
        /// <param name="results">The system results to analyse.</param>
        /// <param name="dataTransform">The transform required to extract the state from the results.</param>
        /// <param name="targetValue">The controller's target value.</param>
        /// <param name="accuracy">The required accuracy of the controller.</param>
        /// <returns>
        /// An integer representing the fitness of the controller.
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// If the system moves more than <paramref name="accuracy"/> times <paramref name="targetValue"/> below,
        /// zero, the fitness is 0.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// If the system does not reach withing <paramref name="accuracy"/> of <paramref name="targetValue"/>, the
        /// fitness is 10.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// If the system overshoots by more than 18%, the fitness is 20.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Otherwise, returns 1000 plus the speed of the system, where the speed is the length of the simulation
        /// minus the number of cycles it takes for the simulation to settle to within <paramref name="accuracy"/>
        /// of <paramref name="targetValue"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        private static int GenericFitnessCalculator(IEnumerable<SystemState> results, Func<SystemState, double> dataTransform, double targetValue, double accuracy)
        {
            int fitness;

            if (results.Last().OvershootFail)
                // The simulation detected an overshoot. Don't bother calculating fitness.
                return 20;

            var data = results.Select(dataTransform).ToList();

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
                fitness = 1000;
                for (var i = data.Count - 1; i >= 0; i--)
                {
                    if (data[i] <= targetValue * (1 + accuracy) && data[i] >= targetValue * (1 - accuracy)) continue;
                    fitness = 1000  + (data.Count - i);
                    break;
                }
            }

            return fitness;
        }

        /// <summary>
        /// Calculates the fitness of a genome when performing vertical movement.
        /// </summary>
        /// <param name="results">The system results to analyse.</param>
        /// <param name="targetX">The target X coordinate (unused).</param>
        /// <param name="targetY">The target Y coordinate.</param>
        /// <param name="targetAngle">The target angle (unused).</param>
        /// <param name="accuracy">The required accuracy accuracy.</param>
        /// <returns>An integer representing the fitness of the genome.  Higher numbers are better.</returns>
        public static int VerticalFitnessCalculator(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy)
        {
            return GenericFitnessCalculator(results, ss => ss.Y, targetY, accuracy);
        }
    }
}
