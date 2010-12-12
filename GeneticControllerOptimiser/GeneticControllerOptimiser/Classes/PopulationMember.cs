using System;
using System.Collections.Generic;

namespace GeneticControllerOptimiser.Classes
{
    /// <summary>
    /// Delegate used to pass around fitness functions in a generic fashion.
    /// </summary>
    /// <param name="results">The system results to analyse.</param>
    /// <param name="targetX">The target X coordinate.</param>
    /// <param name="targetY">The target Y coordinate.</param>
    /// <param name="targetAngle">The target angle.</param>
    /// <param name="accuracy">The required accuracy.</param>
    /// <returns>An integer representing the fitness of the genome.  Higher numbers are better.</returns>
    internal delegate int FitnessFunction(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy);

    /// <summary>
    /// Enumeration to show which variables the function is interested in.
    /// </summary>
    [Flags]
    enum TargetVariables
    {
        /// <summary>
        /// Vertical movement
        /// </summary>
        Vertical = 1,
        /// <summary>
        /// Angle movement
        /// </summary>
        Angle = 2,
        /// <summary>
        /// Horizontal movement
        /// </summary>
        Horizontal = 4
    }

    /// <summary>
    /// Represents a member of the population, with it's associated genome, controllers, systems
    /// and results.
    /// </summary>
    class PopulationMember
    {
        /// <summary>
        /// A cache of previous system results to speed up recalculating reused
        /// genomes.
        /// </summary>
        /// <value>The past results.</value>
        public static Dictionary<Genome, SystemResults> PastResults { get; set; }

        /// <summary>
        /// Gets or sets the member's genome.
        /// </summary>
        /// <value>The genome.</value>
        public Genome Genome { get; set; }
        /// <summary>
        /// Gets or sets the results from positive movement.
        /// </summary>
        /// <value>The results as a List.</value>
        public List<SystemState> Results { get; set; }
        /// <summary>
        /// Gets or sets the results from negative movement.
        /// </summary>
        /// <value>The negative results as a List.</value>
        /// <remarks>This is only populated if negative movement was attempted</remarks>
        public List<SystemState> NegativeResults { get; set; }
        /// <summary>
        /// Gets or sets the controller used for positive movement.
        /// </summary>
        /// <value>The controller.</value>
        public Controller Controller { get; set; }
        /// <summary>
        /// Gets or sets the controller used for negative movement.
        /// </summary>
        /// <value>The negative controller.</value>
        public Controller NegativeController { get; set; }
        /// <summary>
        /// Gets or sets the system used for positive movement testing.
        /// </summary>
        /// <value>The system.</value>
        public System System { get; set; }
        /// <summary>
        /// Gets or sets the system used for negative movement testing.
        /// </summary>
        /// <value>The negative system.</value>
        public System NegativeSystem { get; set; }
        /// <summary>
        /// Gets or sets the fitness of the positive movement.
        /// </summary>
        /// <value>An integer representing the fitness, where large numbers are better.</value>
        public int Fitness { get; set; }
        /// <summary>
        /// Gets or sets the fitness of the negative movement.
        /// </summary>
        /// <value>An integer representing the fitness, where large numbers are better.</value>
        public int NegativeFitness { get; set; }

        /// <summary>
        /// Initializes the <see cref="PopulationMember"/> class.
        /// </summary>
        /// <remarks>
        /// Sets up the results cache.
        /// </remarks>
        static PopulationMember()
        {
            PastResults = new Dictionary<Genome, SystemResults>();
        }

        /// <summary>
        /// Runs the simulations for <paramref name="cycleCount"/> cycles.
        /// </summary>
        /// <param name="cycleCount">The number of cycles to run for.</param>
        /// <param name="targetVariable">The target variable.</param>
        /// <param name="targetX">The target X coordinate.</param>
        /// <param name="targetY">The target Y coordinate.</param>
        /// <param name="targetAngle">The target angle.</param>
        /// <param name="fitnessFunction">The fitness function to use.</param>
        /// <param name="accuracy">The target accuracy.</param>
        /// <param name="doNegative">if set to <c>true</c>, perform negative movement testing.</param>
        /// <param name="minusTargetX">The negative target X coordinate.</param>
        /// <param name="minusTargetY">The negative target Y coordinate.</param>
        /// <param name="minusTargetAngle">The negative target angle.</param>
        /// <returns>A list containing the genome and the fitness of the system in each tested direction.</returns>
        /// <remarks>
        /// The simulation is run by calling <see cref="Classes.Controller.Process" />, and then <see cref="Classes.System.Process"/>
        /// in turn.  If the <see cref="Classes.System"/> detects that the 
        /// pod has overshot, the processing finishes early.  This is permitted because the fitness function
        /// does not require any more information to calculate the fitness.
        /// 
        /// This function caches previously calculated values.  If the precise genome has already been
        /// processed, it's fitness is retrieved from the cache instead of being recalculated.
        /// </remarks>
        public PopulationMember Process(int cycleCount, TargetVariables targetVariable, double targetX, double targetY, double targetAngle, FitnessFunction fitnessFunction, double accuracy, bool doNegative = false, double minusTargetX = 0, double minusTargetY = 0, double minusTargetAngle = 0)
        {
            var output = new PopulationMember();

            if (PastResults.ContainsKey(Genome))
            {
                output.Genome = Genome;
                output.Fitness = PastResults[Genome].Fitness;
                output.NegativeFitness = PastResults[Genome].NegativeFitness;

                return output;
            }

            var systemState = new SystemState();
            var targetState = new TargetState { TargetX = targetX, TargetY = targetY, TargetAngle = targetAngle };

            if (targetVariable.HasFlag(TargetVariables.Angle))
                targetState.AngleCutOff = targetAngle * 1.18;
            if (targetVariable.HasFlag(TargetVariables.Vertical))
                targetState.YCutOff = targetY * 1.18;
            if (targetVariable.HasFlag(TargetVariables.Horizontal))
                targetState.XCutOff= targetX * 1.18;

            var result = new List<SystemState>();
            for (var i = 0; i < cycleCount; i++)
            {
                var thrusterState = Controller.Process(systemState, targetX, targetY, targetAngle);
                systemState = System.Process(thrusterState, targetState);
                result.Add(systemState);
                if (systemState.OvershootFail)
                    break;
            }

            output.Controller = Controller;
            output.Fitness = fitnessFunction(result, targetX, targetY, Math.PI - targetAngle, accuracy);
            output.Genome = Genome;
            output.Results = result;
            output.System = System;

            if (doNegative)
            {
                var negSystemState = new SystemState();
                var negResult = new List<SystemState>();
                var negTargetState = new TargetState { TargetAngle = minusTargetAngle, TargetX = minusTargetX, TargetY = minusTargetY };
                if (targetVariable.HasFlag(TargetVariables.Angle))
                    targetState.AngleCutOff = Math.Abs(minusTargetAngle) * 1.18;
                if (targetVariable.HasFlag(TargetVariables.Vertical))
                    targetState.YCutOff = Math.Abs(minusTargetY) * 1.18;
                if (targetVariable.HasFlag(TargetVariables.Horizontal))
                    targetState.XCutOff = Math.Abs(minusTargetX) * 1.18;

                for (var i = 0; i < cycleCount; i++)
                {
                    var thrusterState = NegativeController.Process(negSystemState, minusTargetX, minusTargetY, minusTargetAngle);
                    negSystemState = NegativeSystem.Process(thrusterState, negTargetState);
                    negResult.Add(negSystemState);
                }

                output.NegativeController = NegativeController;
                output.NegativeFitness = fitnessFunction(negResult, minusTargetX, minusTargetY, Math.PI - minusTargetAngle, accuracy);
                output.NegativeResults = result;
                output.NegativeSystem = System;
            }

            lock (PastResults)
            {
                PastResults.Add(Genome, new SystemResults { Fitness = output.Fitness, NegativeFitness = output.NegativeFitness });
            }

            return output;
        }
    }
}
