namespace MultiAgentLibrary
{
    using System.Drawing;

    /// <summary>
    /// A sensor reading describing the data retrieved from a file.
    /// </summary>
    public struct SensorReading
    {
        /// <summary>
        /// Gets or sets the range of the detected object.
        /// </summary>
        /// <value>The range of the reading.</value>
        public float Range { get; set; }

        /// <summary>
        /// Gets or sets the angle that the sensor is looking in.
        /// </summary>
        /// <value>The sensor angle.</value>
        public float Angle { get; set; }

        /// <summary>
        /// Gets or sets the state of the sensor, identifying what it has detected.
        /// </summary>
        /// <value>The state of the sensor.</value>
        public SensorState State { get; set; }

        /// <summary>
        /// Gets or sets the position of the sensor.
        /// </summary>
        /// <value>The origin of the sensor reading.</value>
        public PointF Origin { get; set; }
    }
}