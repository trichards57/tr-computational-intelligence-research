namespace MultiAgentLab.Classes
{
    using System.Windows;

    struct SensorReading
    {
        public double Range { get; set; }

        public double Angle { get; set; }

        public SensorState State { get; set; }

        public Point Origin { get; set; }
    }
}