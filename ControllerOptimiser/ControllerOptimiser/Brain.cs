using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControllerOptimiser
{
    class Brain
    {
        public double LastSideForce { get; set; }

        public double TargetAngle { get; set; }

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

        public double UpThrust { get; set; }

        public Brain()
        {
            BigYSpeed = 20;
            MidYSpeed = 5;
            SmlYSpeed = 2.5;

            BigXSpeed = 20;
            MidXSpeed = 5;
            SmlXSpeed = 2.5;

            BigYError = 50;
            MidYError = 20;

            BigXError = 50;
            MidXError = 20;

            UpForce = 0.3;
            DownForce = 0.1;
            PropelAngle = 0.1;

            UpThrust = 0.5;
        }

        public List<double> Genome { get; set; }

        public Brain(List<double> genome)
        {
            Genome = genome;

            BigYSpeed = genome[0];
            MidYSpeed = genome[1];
            SmlYSpeed = genome[2];

            BigXSpeed = genome[3];
            MidXSpeed = genome[4];
            SmlXSpeed = genome[5];

            BigYError = genome[6];
            MidYError = genome[7];

            BigXError = genome[8];
            MidXError = genome[9];

            UpForce = 0.4;
            DownForce = 0;
            PropelAngle = genome[12];

            UpThrust = genome[13];
        }


        public Control Process(State state, double dt)
        {
            var control = new Control();
            var error = state.TargetLocation - state.Location;
            var normalisedAngle = (2 * Math.PI) - ((state.Angle + Math.PI) % (2 * Math.PI));
            if (normalisedAngle > Math.PI)
                normalisedAngle -= 2 * Math.PI;

            if (error.Y < 0)
                control.Up = UpThrust;

            double maxSpeed;

            if (Math.Abs(error.Y) > BigYError)
                maxSpeed = BigYSpeed;
            else if (Math.Abs(error.Y) > MidYError)
                maxSpeed = MidYSpeed;
            else
                maxSpeed = SmlYSpeed;

            if (state.Velocity.Y < -maxSpeed)
                control.Up = DownForce;
            else if (state.Velocity.Y > maxSpeed)
                control.Up = UpForce;

            if (error.X > 0)
                TargetAngle = PropelAngle;
            else if (error.X < 0)
                TargetAngle = -PropelAngle;

            if (Math.Abs(error.X) > BigXError)
                maxSpeed = BigXSpeed;
            else if (Math.Abs(error.X) > MidXError)
                maxSpeed = MidXSpeed;
            else
                maxSpeed = SmlXSpeed;

            if (state.Velocity.X > maxSpeed)
                TargetAngle = -PropelAngle;
            else if (state.Velocity.X < -maxSpeed)
                TargetAngle = PropelAngle;

            var angError = TargetAngle - normalisedAngle;
            var sideForce = angError * 6 + state.DAngleDt * 5;

            if (sideForce > 0)
            {
                control.Right = sideForce;
                control.Left = 0;
            }
            else
            {
                control.Right = 0;
                control.Left = -sideForce;
            }

            LastSideForce = sideForce;

            return control;
        }
    }
}
