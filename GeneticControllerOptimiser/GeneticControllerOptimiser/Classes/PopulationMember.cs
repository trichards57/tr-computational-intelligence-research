using System;
using System.Collections.Generic;

namespace GeneticControllerOptimiser.Classes
{
    internal delegate int FitnessFunction(IList<SystemState> results, double targetX, double targetY, double targetAngle, double accuracy);

    [Flags]
    enum TargetVariables
    {
        Vertical = 1,
        Angle = 2,
        Horizontal = 4
    }

    class PopulationMember
    {
        public static Dictionary<Genome, SystemResults> PastResults { get; set; }

        public Genome Genome { get; set; }
        public List<SystemState> Results { get; set; }
        public List<SystemState> NegativeResults { get; set; }
        public Controller Controller { get; set; }
        public Controller NegativeController { get; set; }
        public System System { get; set; }
        public System NegativeSystem { get; set; }
        public int Fitness { get; set; }
        public int NegativeFitness { get; set; }

        static PopulationMember()
        {
            PastResults = new Dictionary<Genome, SystemResults>();
        }

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
            var targetState = new TargetState() { TargetX = targetX, TargetY = targetY, TargetAngle = targetAngle };

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
                var negTargetState = new TargetState() { TargetAngle = minusTargetAngle, TargetX = minusTargetX, TargetY = minusTargetY };
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
                PastResults.Add(Genome, new SystemResults() { Fitness = output.Fitness, NegativeFitness = output.NegativeFitness });
            }

            return output;
        }
    }
}
