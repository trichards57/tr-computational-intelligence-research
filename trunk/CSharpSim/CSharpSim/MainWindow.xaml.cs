using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSharpSim
{
    using CSharpSim.Classes;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var world = new World(@"C:\Users\Tony\Documents\Computational Intelligance Program\world.txt", null);

            MainCanvas.Width = world.Rectangle.Width;
            MainCanvas.Height = world.Rectangle.Height;

            foreach (var wall in world.Walls)
            {
                var color = Colors.Red;
                if (wall.Name == "end")
                    color = Colors.Cyan;

                var segments = wall.Segments.Select(s => new LineSegment(s.Point2, true));
                var f = new PathFigure {
                    StartPoint = wall.Segments.First().Point1,
                    Segments = new PathSegmentCollection(segments)
                };

                f.Segments.Add(new LineSegment(wall.Segments.Last().Point1, true));

                var geometry = new PathGeometry();
                geometry.Figures.Add(f);
                var path = new Path {
                    Data = geometry,
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 6
                };

                MainCanvas.Children.Add(path);
            }
        }
    }
}
