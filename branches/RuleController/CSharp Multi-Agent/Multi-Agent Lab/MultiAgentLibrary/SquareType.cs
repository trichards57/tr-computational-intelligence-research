namespace MultiAgentLibrary
{
    /// <summary>
    /// Represents the type of points in each square.
    /// </summary>
    public enum SquareType
    {
        /// <summary>
        /// The square only contains passable points.
        /// </summary>
        Passable,
        /// <summary>
        /// The square contains at least one wall point, and no destination points.
        /// </summary>
        Wall,
        /// <summary>
        /// The square contains at least one destination point.
        /// </summary>
        Destination
    }
}
