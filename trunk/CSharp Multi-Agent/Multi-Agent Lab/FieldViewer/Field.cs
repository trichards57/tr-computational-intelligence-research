using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Multi_Agent_Lab
{
    /// <summary>
    /// Represents a square in the field, which has a given pheremone level, travel speed and a marker
    /// to show if the square is passable.
    /// </summary>
    class FieldSquare
    {
        private double _pheremoneLevel = 0;
        private double _travelSpeed = 1;

        /// <summary>
        /// Gets or sets the pheromone level of the square.  Must be greater than 0.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the level is set to a value less than 0.</exception>
        /// <value>The pheromone level.</value>
        public double PheromoneLevel 
        {
            get
            {
                return _pheremoneLevel;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "PheromoneLevel must be greater than or equal to 0.");

                _pheremoneLevel = value;
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FieldSquare"/> is passable.
        /// </summary>
        /// <value><c>true</c> if passable; otherwise, <c>false</c>.</value>
        public bool Passable { get; set; }

        /// <summary>
        /// Gets or sets the travel speed an agent will experience through this square.
        /// </summary>
        /// <value>The travel speed.</value>
        public double TravelSpeed 
        { 
            get
            {
                return _travelSpeed;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "TravelSpeed must be greater than 0.");

                _travelSpeed = value;
            }
        }

        /// <summary>
        /// Gets the position of the square in the field.
        /// </summary>
        /// <value>The position.</value>
        public Point Position { get; private set; }

        private static Random rand = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSquare"/> class.
        /// </summary>
        public FieldSquare(Point position)
        {
            if (position == null)
                throw new ArgumentNullException("position");
            PheromoneLevel = rand.NextDouble() * 10;
            Position = position;
        }
    }

    /// <summary>
    /// Represents a field an agent moves around.
    /// </summary>
    class Field
    {
        /// <summary>
        /// Gets or sets the squares on the field.
        /// </summary>
        /// <value>The squares.</value>
        public List<FieldSquare> Squares { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="fieldSize">Size of the field.</param>
        public Field(Size fieldSize)
        {
            Squares = new List<FieldSquare>(fieldSize.Width);

            for (var i = 0; i < fieldSize.Width; i++)
            {
                for (var j = 0; j < fieldSize.Height; j++)
                    Squares.Add(new FieldSquare(new Point(i, j)));
            }
        }
    }
}
