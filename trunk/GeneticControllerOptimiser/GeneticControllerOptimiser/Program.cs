using System;
using System.Collections.Generic;
using System.Linq;
using GeneticControllerOptimiser.Classes;

namespace GeneticControllerOptimiser
{
    using System.Threading.Tasks;

    class Program
    {
        private const int GenomeCount = 1000;
        private const double TargetAngle = Math.PI / 4;
        private const double MinusTargetAngle = -TargetAngle;
        private const double TargetY = 20;
        private const double TargetX = 20;
        private const double MinusTargetY = -TargetY;
        private const double MinusTargetX = -TargetX;
        private const double MutationRate = 0.75;
        private static readonly Random Rand = new Random();

        static void Main()
        {
            //var testGenome = new[] { 20, 5, 2.5, 20, 5, 2.5, 50, 20, 50, 20, 0.3, 0.1, 0.1, 6, 5, 0 };

            var range = ParallelEnumerable.Range(0, GenomeCount);

            var list = range.Select(i =>
                {
                    var ret = new List<double>();
                    for (var j = 0; j < 16; j++)
                    {
                        ret.Add(MultiRandom.NextDouble() * 100 - 50);
                    }
                    return ret;
                });

            var population = CreatePopulation(list);

            int fitness;

            IList<double> topGenome;

            do
            {
                var results = population.Select(p =>
                    {
                        var systemState = new SystemState();
                        var result = new List<SystemState> { systemState };
                        for (var i = 0; i < 500; i++)
                        {
                            var thrusterState = p.Controller.Process(systemState, 0, 0, Math.PI - TargetAngle);
                            systemState = p.System.Process(thrusterState);
                            result.Add(systemState);
                        }
                        return new { ResultData = result, p.Controller, p.System, Fitness = AngleFitnessCalculator(result, TargetAngle), p.Genome };
                    }).OrderByDescending(r => r.Fitness).ToList();

                Console.WriteLine("Top Fitness     : {0}", results.Max(a => a.Fitness));
                Console.WriteLine("Average Fitness : {0}", results.Average(a => a.Fitness));

                var topThreeQuarterGenomes = results.Take(3 * GenomeCount / 4).Select(p => p.Genome).ToList();
                var topQuarterGenomes = topThreeQuarterGenomes.Take(GenomeCount / 4);

                var children = from pair in
                                   (from g in topQuarterGenomes
                                    select new
                                    {
                                        Parent1 = g,
                                        Parent2 = topThreeQuarterGenomes[Rand.Next(0, topThreeQuarterGenomes.Count)]
                                    })
                               select Breed(pair.Parent1, pair.Parent2);
                list = topThreeQuarterGenomes.Union(children).ToList().AsParallel().AsOrdered();

                Parallel.ForEach(list.Skip(GenomeCount / 4).Where(g => MultiRandom.NextDouble() < MutationRate), g => MutateAngle(g));

                fitness = results.First().Fitness;
                topGenome = results.First().Genome;

                population = CreatePopulation(list);

            } while (!Console.KeyAvailable && fitness < 960);

            var bestAngleGenes = topGenome.Skip(13);

            list = range.Select(i =>
            {
                var ret = new List<double>();
                for (var j = 0; j < 13; j++)
                {
                    ret.Add(MultiRandom.NextDouble() * 100 - 50);
                }

                ret.AddRange(bestAngleGenes);

                return ret;
            });

            population = CreatePopulation(list, false, Math.PI);

            do
            {
                var results = population.Select(p =>
                {
                    var systemState = new SystemState();
                    var negSystemState = new SystemState();
                    var result = new List<SystemState> { systemState };
                    var negResults = new List<SystemState> { negSystemState };
                    for (var i = 0; i < 500; i++)
                    {
                        var thrusterState = p.Controller.Process(systemState, 0, TargetY, Math.PI);
                        var negThrustState = p.NegativeController.Process(negSystemState, 0, MinusTargetY, Math.PI);
                        systemState = p.System.Process(thrusterState);
                        negSystemState = p.NegativeSystem.Process(negThrustState);
                        result.Add(systemState);
                        negResults.Add(negSystemState);
                    }
                    return new
                    {
                        NegativeResult = negResults,
                        p.NegativeController,
                        p.NegativeSystem,
                        ResultData = result,
                        p.Controller,
                        p.System,
                        PositiveFitness = VerticalFitnessCalculator(result, TargetY),
                        NegativeFitness = VerticalFitnessCalculator(negResults, MinusTargetY),
                        p.Genome
                    };
                }).OrderByDescending(r => (r.PositiveFitness + r.NegativeFitness) / 2).ToList();

                Console.WriteLine("Top Fitness     : {0}", results.Max(r => (r.PositiveFitness + r.NegativeFitness) / 2));
                Console.WriteLine("Average Fitness : {0}", results.Average(r => (r.PositiveFitness + r.NegativeFitness) / 2));

                var topThreeQuarterGenomes = results.Take(3 * GenomeCount / 4).Select(p => p.Genome).ToList();
                var topQuarterGenomes = topThreeQuarterGenomes.Take(GenomeCount / 4);

                var children = from pair in
                                   (from g in topQuarterGenomes
                                    select new
                                    {
                                        Parent1 = g,
                                        Parent2 = topThreeQuarterGenomes[Rand.Next(0, topThreeQuarterGenomes.Count)]
                                    })
                               select Breed(pair.Parent1, pair.Parent2);
                list = topThreeQuarterGenomes.Union(children).ToList().AsParallel().AsOrdered();

                Parallel.ForEach(list.Skip(GenomeCount / 4).Where(g => MultiRandom.NextDouble() < MutationRate), g => MutateYControl(g));

                fitness =  (results.First().NegativeFitness + results.First().PositiveFitness) / 2;
                topGenome = results.First().Genome;

                var bestResult = results.First().ResultData.Select(r => r.Y);
                var bestResultString = string.Join(",", bestResult);

                var negBestResult = results.First().NegativeResult.Select(r => r.Y);
                var negBestResultString = string.Join(",", negBestResult);

                population = CreatePopulation(list, false, Math.PI);

            } while (!Console.KeyAvailable && fitness < 900);

            var bestYGenes = topGenome.Skip(6).Take(7);

            list = range.Select(i =>
            {
                var ret = new List<double>();
                for (var j = 0; j < 6; j++)
                {
                    ret.Add(MultiRandom.NextDouble() * 100 - 50);
                }
                ret.AddRange(bestYGenes);
                ret.AddRange(bestAngleGenes);

                return ret; 
            });

            population = CreatePopulation(list, false, Math.PI);

            do
            {
                var results = population.Select(p =>
                {
                    var systemState = new SystemState();
                    var negSystemState = new SystemState();
                    var result = new List<SystemState> { systemState };
                    var negResults = new List<SystemState> { negSystemState };
                    for (var i = 0; i < 1000; i++)
                    {
                        var thrusterState = p.Controller.Process(systemState, TargetX, 0, Math.PI);
                        var negThrustState = p.NegativeController.Process(negSystemState, MinusTargetX, 0, Math.PI);
                        systemState = p.System.Process(thrusterState);
                        negSystemState = p.NegativeSystem.Process(negThrustState);
                        result.Add(systemState);
                        negResults.Add(negSystemState);
                    }
                    return new
                    {
                        NegativeResult = negResults,
                        p.NegativeController,
                        p.NegativeSystem,
                        ResultData = result,
                        p.Controller,
                        p.System,
                        PositiveFitness = HorizontalFitnessCalculator(result, TargetX),
                        NegativeFitness = HorizontalFitnessCalculator(negResults, MinusTargetX),
                        p.Genome
                    };
                }).OrderByDescending(r => (r.PositiveFitness + r.NegativeFitness) / 2).ToList();

                Console.WriteLine("Top Fitness     : {0}", results.Max(r => (r.PositiveFitness + r.NegativeFitness) / 2));
                Console.WriteLine("Average Fitness : {0}", results.Average(r => (r.PositiveFitness + r.NegativeFitness) / 2));

                var topThreeQuarterGenomes = results.Take(3 * GenomeCount / 4).Select(p => p.Genome).ToList();
                var topQuarterGenomes = topThreeQuarterGenomes.Take(GenomeCount / 4);

                var children = from pair in
                                   (from g in topQuarterGenomes
                                    select new
                                    {
                                        Parent1 = g,
                                        Parent2 = topThreeQuarterGenomes[Rand.Next(0, topThreeQuarterGenomes.Count)]
                                    })
                               select Breed(pair.Parent1, pair.Parent2);
                list = topThreeQuarterGenomes.Union(children).ToList().AsParallel().AsOrdered();

                Parallel.ForEach(list.Skip(GenomeCount / 4).Where(g => MultiRandom.NextDouble() < MutationRate), g => MutateXControl(g));

                fitness = (results.First().NegativeFitness + results.First().PositiveFitness) / 2;
                topGenome = results.First().Genome;

                var bestResult = results.First().ResultData.Select(r => r.X);
                var bestResultString = string.Join(",", bestResult);

                var negBestResult = results.First().NegativeResult.Select(r => r.X);
                var negBestResultString = string.Join(",", negBestResult);

                population = CreatePopulation(list, false, Math.PI);

            } while (!Console.KeyAvailable);
        }

        private static object MutateXControl(IList<double> item)
        {
            for (var i = 0; i < 6; i++)
            {
                if (MultiRandom.NextDouble() < MutationRate)
                    item[i] = MultiRandom.NextDouble() * 100 - 50;
            }

            return item;
        }

        private static int HorizontalFitnessCalculator(IList<SystemState> results, double targetX)
        {
            int fitness;

            var data = targetX < 0 ? results.Select(r => -r.X).ToList() : results.Select(r => r.X).ToList();
            targetX = Math.Abs(targetX);

            if (Math.Sign(data.First(d => d != 0)) != Math.Sign(targetX))
            {
                // Immediatly started heading the wrong way.  No good at all.
                fitness = 0;
            }
            else if (data.Min() < 0)
            {
                // Must be unstable as it oscillates the wrong way.
                fitness = 0;
            }
            else if (data.Max() < targetX * 0.95)
            {
                // Does not reach the target y (5% error allowed).
                fitness = 10;
            }
            else if (data.Max() > targetX * 1.18)
            {
                // Overshoots too far.  Not very good. (18% error allowed.)
                fitness = 20;
            }
            else
            {
                var speed = 1000 - results.IndexOf(results.First(s =>
                {
                    var currentIndex = results.IndexOf(s);
                    return data.Skip(currentIndex + 1).All(r => r > targetX * 0.95 && r < targetX * 1.05);
                }));
                fitness = speed;
            }

            return fitness;
        }

        private static ParallelQuery<PopulationMember> CreatePopulation(IEnumerable<List<double>> genomeList, bool disableGravity = true, double initialAngle = 0.0)
        {
            return genomeList.AsParallel().AsOrdered().Select(g => new PopulationMember { Genome = g, Controller = Controller.FromGenome(g.ToArray()), System = new Classes.System { DisableGravity = disableGravity, Angle = initialAngle }, NegativeController = Controller.FromGenome(g.ToArray()), NegativeSystem = new Classes.System { DisableGravity = disableGravity, Angle = initialAngle } });
        }

        /// <summary>
        /// Calculates the fitness for the given system results.
        /// </summary>
        /// <param name="results">The results to analyse.</param>
        /// <param name="targetAngle">The target angle.</param>
        /// <returns></returns>
        private static int AngleFitnessCalculator(IList<SystemState> results, double targetAngle)
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
            else if (results.Max(s => s.Angle) < targetAngle * 0.99)
            {
                // Does not reach the target angle (1% error allowed).
                fitness = 10;
            }
            else if (results.Max(s => s.Angle) > targetAngle * 1.18)
            {
                // Overshoots too far.  Not very good. (18% error allowed.)
                fitness = 20;
            }
            else
            {
                var speed = 1000 - results.IndexOf(results.First(s =>
                {
                    var currentIndex = results.IndexOf(s);
                    return results.Skip(currentIndex + 1).All(r => r.Angle > targetAngle * 0.99 && r.Angle < targetAngle * 1.01);
                }));
                fitness = speed;
            }

            return fitness;
        }

        private static int VerticalFitnessCalculator(IList<SystemState> results, double targetY)
        {
            int fitness;

            var data = targetY < 0 ? results.Select(r => -r.Y).ToList() : results.Select(r => r.Y).ToList();
            targetY = Math.Abs(targetY);

            if (Math.Sign(data[2]) != Math.Sign(targetY))
            {
                // Immediatly started heading the wrong way.  No good at all.
                fitness = 0;
            }
            else if (data.Min() < 0)
            {
                // Must be unstable as it oscillates the wrong way.
                fitness = 0;
            }
            else if (data.Max() < targetY * 0.95)
            {
                // Does not reach the target y (5% error allowed).
                fitness = 10;
            }
            else if (data.Max() > targetY * 1.18)
            {
                // Overshoots too far.  Not very good. (18% error allowed.)
                fitness = 20;
            }
            else
            {
                var speed = 1000 - results.IndexOf(results.First(s =>
                {
                    var currentIndex = results.IndexOf(s);
                    return data.Skip(currentIndex + 1).All(r => r > targetY * 0.95 && r < targetY * 1.05);
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
            var index = MultiRandom.Next(13, item.Count);

            item[index] = MultiRandom.NextDouble() * 100 - 50;

            return item;
        }

        public static List<double> MutateYControl(List<double> item)
        {
            for (var i = 6; i < 13; i++)
            {
                if (MultiRandom.NextDouble() < MutationRate)
                    item[i] = MultiRandom.NextDouble() * 100 - 50;
            }
            
            return item;
        }
    }
}
