namespace MultiAgentLibrary
{
    using System.Drawing;

    public struct SensorReading
    {
        public float Range { get; set; }

        public float Angle { get; set; }

        public SensorState State { get; set; }

        public PointF Origin { get; set; }
    }
}