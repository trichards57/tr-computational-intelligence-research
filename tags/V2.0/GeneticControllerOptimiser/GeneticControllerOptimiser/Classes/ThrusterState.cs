namespace GeneticControllerOptimiser.Classes
{
    class ThrusterState
    {
        public double Up { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }

        public void Limit()
        {
            if (Up > 1)
                Up = 1;
            if (Up < 0)
                Up = 0;
            if (Left > 1)
                Left = 1;
            if (Left < 0)
                Left = 0;
            if (Right > 1)
                Right = 1;
            if (Right < 0)
                Right = 0;
        }
    }
}
