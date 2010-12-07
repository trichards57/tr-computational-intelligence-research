using System;
using System.Drawing;

namespace MultiAgentLibrary
{
    /// <summary>
    /// Represents one square in the agent's environment.
    /// </summary>
    public class FieldSquare
    {
        /// <summary>
        /// The pheremone level that is added to a route when an agent reaches the destination.
        /// </summary>
        public const int SuccessPheromoneLevel = 1000;

        /// <summary>
        /// The amount that the pheremone level in each passable square decays each cycle.
        /// </summary>
        public const int PheromoneDecayRate = (SuccessPheromoneLevel) / 1000;

        /// <summary>
        /// Backing field for the <see cref="PheromoneLevel"/> property.
        /// </summary>
        private uint pheromoneLevel;

        /// <summary>
        /// The maximum level the <see cref="PheromoneLevel"/> property will accept.
        /// </summary>
        public const uint MaxPheromoneLevel = uint.MaxValue;

        /// <summary>
        /// Backing field for the <see cref="LockObject"/> property.
        /// </summary>
        private readonly object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSquare"/> class at the given point.
        /// </summary>
        /// <param name="position">The position of the square in the Field.</param>
        /// <remarks>
        /// Initialises the <see cref="Position"/> of the square to <paramref name="position"/>, and the 
        /// <see cref="PheromoneLevel"/> to 1.
        /// </remarks>
        public FieldSquare(Point position)
        {
            Position = position;
            PheromoneLevel = 1;
        }

        /// <summary>
        /// Gets or sets the position of the square in the environment.
        /// </summary>
        /// <value>The position of the square.</value>
        public Point Position { get; private set; }

        /// <summary>
        /// Gets the color used to represent the square in an image.
        /// </summary>
        /// <value>The colour of the square.</value>
        /// <remarks>
        /// A square is shown as red if it is a wall, or white if it is a destination.
        /// Otherwise, the square is a level of green, where the green component is 
        /// proportional to the log of the <see cref="PheromoneLevel"/>.
        /// 
        /// This is used when images of the state of the environment are required.
        /// </remarks>
        public Color SquareColour
        {
            get
            {
                switch (SquareType)
                {
                    case SquareType.Wall:
                        return Color.Red;
                    case SquareType.Destination:
                        return Color.White;
                    default:
                        var level = (byte)(Math.Round((255.0 / Math.Log(MaxPheromoneLevel)) * Math.Log(pheromoneLevel)));
                        return Color.FromArgb(0, level, 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets the pheromone level of the square.
        /// </summary>
        /// <value>The pheromone level of the square.</value>
        /// <remarks>
        /// The pheromone level of the square influences the agents that are passing nearby.  A high
        /// pheromone level compared to the rest of the agent's neighbours makes it more likely that 
        /// an agent will move on to this square next.
        /// </remarks>
        public uint PheromoneLevel
        {
            get
            {
                return pheromoneLevel;
            }
            set
            {
                if (value > MaxPheromoneLevel)
                    pheromoneLevel = MaxPheromoneLevel;
                else if (value < 0)
                    pheromoneLevel = 0;
                else
                    pheromoneLevel = value;
            }
        }

        /// <summary>
        /// The backing field for the <see cref="SquareType"/> field.
        /// </summary>
        private SquareType type;

        /// <summary>
        /// Gets or sets the type of the square.
        /// </summary>
        /// <value>The type of the square.</value>
        /// <remarks>
        /// The type of the square determines the basic pheromone level.  A wall has a pheromone level
        /// of 0. (Removes the need for collision detection.  It is impossible for an agent to chose a
        /// wall over any other passable square). A destination has a pheromone level of 
        /// <see cref="MaxPheromoneLevel"/>, so that it is almost certain to be chosen over any other
        /// square around the agent.  All other squares have a pheromone level of at least 1, which is 
        /// dependent on the number of successful routes that have passed over it and how long ago 
        /// that route was completed.
        /// </remarks>
        public SquareType SquareType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                switch (type)
                {
                    case SquareType.Wall:
                        PheromoneLevel = 0;
                        break;
                    case SquareType.Destination:
                        PheromoneLevel = MaxPheromoneLevel;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the lock object used for thread synchronisation.
        /// </summary>
        /// <value>The lock object.</value>
        public object LockObject
        {
            get
            {
                return lockObject;
            }
        }
    }
}
