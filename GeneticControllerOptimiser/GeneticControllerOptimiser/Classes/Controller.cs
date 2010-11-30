using System;

namespace GeneticControllerOptimiser.Classes
{
    class Controller
    {
        public double VerticalProportionalGain { get; set; }
        public double VerticalDifferentialGain { get; set; }
        public double HorizontalProportionalGain { get; set; }
        public double HorizontalDifferentialGain { get; set; }
        public double AngleProportionalGain { get; set; }
        public double AngleDifferentialGain { get; set; }
        public double AngleGain { get; set; }
        public double LastSideForce { get; set; }
        public double HorizontalForceFeedbackScale { get; set; }

        public ThrusterState Control { get; set; }

        public static Controller FromGenome(double[] genome)
        {
            if (genome.Length != 8)
                throw new ArgumentException("Genome is not the right length.", "genome");
            return new Controller
            {
                HorizontalProportionalGain = genome[0],
                HorizontalDifferentialGain = genome[1],
                VerticalProportionalGain = genome[2],
                VerticalDifferentialGain = genome[3],
                AngleProportionalGain = genome[4],
                AngleDifferentialGain = genome[5],
                AngleGain = genome[6],
                HorizontalForceFeedbackScale = genome[7]
            };
        }

        private Controller()
        {
            Control = new ThrusterState();
        }

        public ThrusterState Process(SystemState state, double targetX, double targetY, double targetAngle)
        {
            Control.Up = targetY - (VerticalProportionalGain * state.Y + VerticalDifferentialGain * state.DyDt);

            var horizontalFeedback = (targetX - (state.X * HorizontalProportionalGain + (HorizontalDifferentialGain * state.DxDt))) / HorizontalForceFeedbackScale;

            if (horizontalFeedback > 1)
                horizontalFeedback = 1;
            else if (horizontalFeedback < -1)
                horizontalFeedback = -1;

            var horizontalForce = (Math.PI - (AngleGain * Math.Asin(horizontalFeedback))) - (state.Angle * AngleProportionalGain + AngleDifferentialGain * state.DAngleDt);

            if (horizontalForce > 0)
            {
                Control.Left = horizontalForce;
                Control.Right = 0;
            }
            else
            {
                Control.Left = 0;
                Control.Right = Math.Abs(horizontalForce);
            }

            Control.Limit();

            return Control;
        }
    }
}
