using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Multi_Agent_Lab
{
    partial class FieldDisplay : Control
    {
        public FieldDisplay()
        {
            InitializeComponent();

            Field = new Field(FieldSize);
        }

        private Size _fieldSize = new Size(50,50);

        [DesignOnly(true)]
        public Size FieldSize 
        {
            get
            {
                return _fieldSize;
            }
            set
            {
                if (this.DesignMode)
                {
                    _fieldSize = value;
                    Field = new Field(_fieldSize);
                    OnFieldSizeChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler FieldSizeChanged;

        protected virtual void OnFieldSizeChanged(EventArgs args)
        {
            if (FieldSizeChanged != null)
            {
                FieldSizeChanged(this, args);
            }
        }

        public Field Field { get; set; }

        protected override void  OnPaintBackground(PaintEventArgs pevent)
        {
 	        base.OnPaintBackground(pevent);
            pevent.Graphics.FillRectangle(Brushes.Green, pevent.ClipRectangle);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            var squareSize = new Size(Size.Width / FieldSize.Width, Size.Height / FieldSize.Height);

            var maxPheromone = Field.Squares.Max(p => p.PheromoneLevel);
            if (maxPheromone < double.Epsilon)
                maxPheromone = 1; // Prevents divide by zero problems when all the squares have zero pheromone.

            var squares = Field.Squares.Select(f => new 
                                                        { 
                                                            Color = f.Passable ? PheremoneColor(maxPheromone, f.PheromoneLevel) : Color.Yellow,
                                                            Rectangle = new Rectangle(new Point(f.Position.X * squareSize.Width, f.Position.Y * squareSize.Height), squareSize),
                                                            
                                                        });

            foreach (var square in squares)
                pe.Graphics.FillRectangle(new SolidBrush(square.Color), square.Rectangle);
        }

        private Color PheremoneColor(double maxPheromone, double squarePheromone)
        {
            var red = (int)Math.Round((squarePheromone / maxPheromone) * 255.0, 0);

            return Color.FromArgb(red, 0, 0);
        }
    }
}
