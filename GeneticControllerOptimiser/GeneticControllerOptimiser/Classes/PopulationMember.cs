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
        public static Dictionary<Genome, SystemResults> PastResults { get; private set; }

        /// <summary>
        /// Gets or sets the member's genome.
        /// </summary>
        /// <value>The genome.</value>
        public Genome Genome { get; set; }
        /// <summary>
        /// Gets or sets the controller used for positive movement.
        /// </summary>
        /// <value>The controller.</value>
        public Controller Controller { private get; set; }
        /// <summary>
        /// Gets or sets the controller used for negative movement.
        /// </summary>
        /// <value>The negative controller.</value>
        public Controller NegativeController { private get; set; }
        /// <summary>
        /// Gets or sets the system used for positive movement testing.
        /// </summary>
        /// <value>The system.</value>
        public System System { private get; set; }
        /// <summary>
        /// Gets or sets the system used for negative movement testing.
        /// </summary>
        /// <value>The negative system.</value>
        public System NegativeSystem { private get; set; }
        /// <summary>
        /// Gets or sets the fitness of the positive movement.
        /// </summary>
        /// <value>An integer representing the fitness, where large numbers are better.</value>
        public int Fitness { get; private set; }
        /// <summary>
        /// Gets or sets the fitness of the negative movement.
        /// </summary>
        /// <value>An integer representing the fitness, where large numbers are better.</value>
        public int NegativeFitness { get; private set; }

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
        /// <param name="targetState">The target state of the system.</param>
        /// <param name="fitnessFunction">The fitness function to use.</param>
        /// <param name="accuracy">The target accuracy.</param>
        /// <param name="negativeTargetState">The negative target state of the system.</param>
        /// <returns>A list containing the genome and the fitness of the system in each tested direction.</returns>
        /// <remarks>
        /// The simulation is run by calling <see cref="Classes.ControllerHelper.Process" />, and then <see cref="Classes.System.Process"/>
        /// in turn.  If the <see cref="Classes.System"/> detects that the 
        /// pod has overshot, the processing finishes early.  This is permitted because the fitness function
        /// does not require any more information to calculate the fitness.
        /// 
        /// This function caches previously calculated values.  If the precise genome has already been
        /// processed, it's fitness is retrieved from the cache instead of being recalculated.
        /// </remarks>
        public PopulationMember Process(int cycleCount, TargetVariables targetVariable, TargetState targetState, FitnessFunction fitnessFunction, double accuracy, TargetState negativeTargetState = null)
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

            if (targetVariable.HasFlag(TargetVariables.Angle))
                targetState.AngleCutOff = targetState.Angle * 1.18;
            if (targetVariable.HasFlag(TargetVariables.Vertical))
                targetState.YCutOff = targetState.Y * 1.18;
            if (targetVariable.HasFlag(TargetVariables.Horizontal))
                targetState.XCutOff = targetState.X * 1.18;

            var result = new List<SystemState>();
            for (var i = 0; i < cycleCount; i++)
            {
                var thrusterState = ControllerHelper.Process(Controller, systemState, targetState);
                systemState = System.Process(thrusterState, targetState);
                result.Add(systemState);
                if (systemState.OvershootFail)
                    break;
            }

            output.Controller = Controller;
            output.Fitness = fitnessFunction(result, targetState.X, targetState.Y, Math.PI - targetState.Angle, accuracy);
            output.Genome = Genome;
            output.System = System;

            if (negativeTargetState != null)
            {
                var negSystemState = new SystemState();
                var negResult = new List<SystemState>();
                
                if (targetVariable.HasFlag(TargetVariables.Angle))
                    negativeTargetState.AngleCutOff = Math.Abs(negativeTargetState.Angle) * 1.18;
                if (targetVariable.HasFlag(TargetVariables.Vertical))
                    negativeTargetState.YCutOff = Math.Abs(negativeTargetState.Y) * 1.18;
                if (targetVariable.HasFlag(TargetVariables.Horizontal))
                    negativeTargetState.XCutOff = Math.Abs(negativeTargetState.X) * 1.18;

                for (var i = 0; i < cycleCount; i++)
                {
                    var thrusterState = ControllerHelper.Process(NegativeController, negSystemState, negativeTargetState);
                    negSystemState = NegativeSystem.Process(thrusterState, negativeTargetState);
                    negResult.Add(negSystemState);
                }

                output.NegativeController = NegativeController;
                output.NegativeFitness = fitnessFunction(negResult, negativeTargetState.X, negativeTargetState.Y, Math.PI - negativeTargetState.Angle, accuracy);
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
