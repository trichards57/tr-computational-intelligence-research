namespace MultiAgentLibrary
{
    /// <summary>
    /// Represents what the sensor has detected.
    /// </summary>
    public enum SensorState
    {
        /// <summary>
        /// The sensor has detected a wall.
        /// </summary>
        Boundary,
        /// <summary>
        /// The sensor has detected the destination.
        /// </summary>
        End,
        /// <summary>
        /// The sensor has not detected anything in it's range.
        /// </summary>
        None
    }
}