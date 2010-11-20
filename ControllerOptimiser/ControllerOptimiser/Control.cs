namespace ControllerOptimiser
{
    class Control
    {
        private static double Limit(double value, double min, double max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public void Limit()
        {
            Up = Limit(Up, 0.0, 1.0);
            Left = Limit(Left, 0.0, 1.0);
            Right = Limit(Right, 0.0, 1.0);
        }

        public double Up { get; set; }

        public double Right { get; set; }

        public double Left { get; set; }
    }
}
