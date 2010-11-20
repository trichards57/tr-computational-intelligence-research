using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControllerOptimiser
{
    using System.Threading.Tasks;
    using System.Windows;

    class Program
    {
        static void Main(string[] args)
        {
            var r = new Random();
            var population = new List<List<double>>();

            for (var i = 0; i < 400; i++)
            {
                var genome = new List<double>();
                for (var j = 0; j < 14; j++)
                {
                    genome.Add(r.NextDouble() * 200);
                }
                population.Add(genome);
            }
            
            while (true)
            {
                var pods = population.Select(g => new Pod(new Brain(g))).ToList();

                Parallel.ForEach(pods, pod =>
                    {
                        for (var i = 0; i < 1000; i++)
                            pod.Step(0.1);
                    });

                var processedPods = pods.Select(p => new
                {
                    Pod = p,
                    Fitness = Math.Abs((p.Location - new Point(30, 30)).Length) + Math.Abs(p.Velocity.Length) * 1000
                }).OrderBy(p => p.Fitness);

                var topQuarter = processedPods.Take(pods.Count() / 4).Select(p => p.Pod.Brain.Genome);

                var newGenes = Breed(topQuarter.ToList());

                population = processedPods.Take(processedPods.Count() - newGenes.Count).Select(p => p.Pod.Brain.Genome).Union(newGenes).ToList();

                Console.WriteLine("Best Pod Position Error : {0}", processedPods.Min(p => p.Fitness));
            }
        }

        static List<List<double>> Breed(IList<List<double>> genome)
        {
            var newGenomes = new List<List<double>>();
            var rand = new Random();

            const double mutationRate = 0.5;

            foreach (var t in genome)
            {
                var g = new List<double>();

                var cutPoint = rand.Next(0, t.Count);
                g.AddRange(t.Take(cutPoint));
                g.AddRange(genome[rand.Next(0, genome.Count)].Skip(cutPoint));

                var mutateChance = rand.NextDouble();

                if (mutateChance < mutationRate)
                {
                    for (var i = 0; i < g.Count; i++)
                        if (rand.NextDouble() < mutationRate)
                            g[i] = rand.NextDouble() * 200;
                }

                newGenomes.Add(g);
            }

            return newGenomes;
        }
    }
}
