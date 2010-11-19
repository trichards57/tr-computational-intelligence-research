using System;

namespace GeneticControllerOptimiser.Classes
{
    class Controller
    {
        public double BigYSpeed { get; set; }
        public double MidYSpeed { get; set; }
        public double SmlYSpeed { get; set; }

        public double BigXSpeed { get; set; }
        public double MidXSpeed { get; set; }
        public double SmlXSpeed { get; set; }

        public double BigXError { get; set; }
        public double MidXError { get; set; }

        public double BigYError { get; set; }
        public double MidYError { get; set; }

        public double UpForce { get; set; }
        public double DownForce { get; set; }

        public double PropelAngle { get; set; }

        public double AngleProportionalGain { get; set; }
        public double AngleDifferentialGain { get; set; }
        public double AngleIntegralGain { get; set; }

        public double LastSideForce { get; set; }
        public double AngleErrorIntegral { get; set; }

        public static Controller FromGenome(double[] genome)
        {
            if (genome.Length != 16)
                throw new ArgumentException("Genome is not the right length.", "genome");
            return new Controller
            {
                BigYSpeed = genome[0],
                MidYSpeed = genome[1],
                SmlYSpeed = genome[2],
                BigXSpeed = genome[3],
                MidXSpeed = genome[4],
                SmlXSpeed = genome[5],
                BigXError = genome[6],
                MidXError = genome[7],
                BigYError = genome[8],
                MidYError = genome[9],
                UpForce = genome[10],
                DownForce = genome[11],
                PropelAngle = genome[12],
                AngleProportionalGain = genome[13],
                AngleDifferentialGain = genome[14],
                AngleIntegralGain = genome[15]
            };
        }

        private Controller()
        {
        }

        public ThrusterState Process(SystemState state, double targetX, double targetY, double targetAngle)
        {
            var control = new ThrusterState();
            var xError = targetX - state.X;
            var yError = targetY - state.Y;

            var normalisedAngle = (2 * Math.PI) - ((state.Angle + Math.PI) % (2 * Math.PI));
            if (normalisedAngle > Math.PI)
                normalisedAngle -= 2 * Math.PI;

            if (yError < 0)
                control.Up = 0.5;

            double maxSpeed;

            if (Math.Abs(yError) > BigYError)
                maxSpeed = BigYSpeed;
            else if (Math.Abs(yError) > MidYError)
                maxSpeed = MidYSpeed;
            else
                maxSpeed = SmlYSpeed;

            if (state.DyDt < -maxSpeed)
                control.Up = DownForce;
            else if (state.DyDt > maxSpeed)
                control.Up = UpForce;

            if (xError > 0)
                targetAngle = PropelAngle;
            else if (xError < 0)
                targetAngle = -PropelAngle;

            if (Math.Abs(xError) > BigXError)
                maxSpeed = BigXSpeed;
            else if (Math.Abs(xError) > MidXError)
                maxSpeed = MidXSpeed;
            else
                maxSpeed = SmlXSpeed;

            if (state.DxDt > maxSpeed)
                targetAngle = -PropelAngle;
            else if (state.DxDt < -maxSpeed)
                targetAngle = PropelAngle;

            var angError = targetAngle - normalisedAngle;

            AngleErrorIntegral += angError;

            var sideForce = (angError * AngleProportionalGain + state.DAngleDt * AngleIntegralGain + AngleErrorIntegral * AngleIntegralGain);

            if (sideForce > 0)
            {
                control.Right = sideForce;
                control.Left = 0;
            }
            else
            {
                control.Left = -sideForce;
                control.Right = 0;
            }

            LastSideForce = sideForce;

            return control;
        }
    }
}
