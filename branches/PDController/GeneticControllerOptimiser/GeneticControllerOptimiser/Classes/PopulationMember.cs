using System;
using System.Collections.Generic;

namespace GeneticControllerOptimiser.Classes
{
    internal delegate int FitnessFunction(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy);

    class PopulationMember
    {
        public List<double> Genome { get; set; }
        public List<SystemState> Results { get; set; }
        public List<SystemState> NegativeResults { get; set; }
        public Controller Controller { get; set; }
        public Controller NegativeController { get; set; }
        public System System { get; set; }
        public System NegativeSystem { get; set; }
        public int Fitness { get; set; }
        public int NegativeFitness { get; set; }

        public PopulationMember Process(int cycleCount, double targetX, double targetY, double targetAngle, FitnessFunction fitnessFunction, double accuracy, bool doNegative = false, double minusTargetX = 0, double minusTargetY = 0, double minusTargetAngle = 0)
        {
            var output = new PopulationMember();

            var systemState = new SystemState();
            var result = new List<SystemState>();
            for (var i = 0; i < cycleCount; i++)
            {
                var thrusterState = Controller.Process(systemState, targetX, targetY, targetAngle);
                systemState = System.Process(thrusterState);
                result.Add(systemState);
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
                for (var i = 0; i < cycleCount; i++)
                {
                    var thrusterState = NegativeController.Process(negSystemState, minusTargetX, minusTargetY, minusTargetAngle);
                    negSystemState = NegativeSystem.Process(thrusterState);
                    negResult.Add(negSystemState);
                }

                output.NegativeController = NegativeController;
                output.NegativeFitness = fitnessFunction(negResult, minusTargetX, minusTargetY, Math.PI - minusTargetAngle, accuracy);
                output.NegativeResults = result;
                output.NegativeSystem = System;
            }

            return output;
        }
    }
}
