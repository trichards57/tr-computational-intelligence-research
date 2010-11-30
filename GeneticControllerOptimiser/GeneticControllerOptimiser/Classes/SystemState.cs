namespace GeneticControllerOptimiser.Classes
{
    class SystemState
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double DxDt { get; set; }
        public double DyDt { get; set; }
        public double Angle { get; set; }
        public double DAngleDt { get; set; }

        public bool OvershootFail { get; set; }

        public override string ToString()
        {
            return string.Format("X {0} Y {1} A {2}", X, Y, Angle);
        }
    }
}
