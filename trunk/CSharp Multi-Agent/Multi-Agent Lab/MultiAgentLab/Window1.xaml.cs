using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using MultiAgentLab.Classes;

namespace MultiAgentLab
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using Rectangle = System.Drawing.Rectangle;
    using Size = System.Windows.Size;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        List<Agent> agentsList = new List<Agent>();
        Timer agentTicker;
        Timer newAgentTicker;
        int count;
        Field field;

        public Window1()
        {
            InitializeComponent();
        }

        private void TimerTick(object state)
        {
            var fild = state as Field;

            if (fild == null)
                throw new ArgumentException("state must be a Field", "state");

            lock (agentsList)
            {
                foreach (var agent in agentsList)
                    agent.Process(fild);
            }

            foreach (var square in fild.SelectMany(row => row.Where(square => square.PheremoneLevel > 1)))
            {
                square.PheremoneLevel -= 0.00001;
            }

            count++;
            if (count > 10000)
            {
                agentsList.Add(new Agent(fild.StartPoint));
                count = 0;
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void LoadMapClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "csv",
                DereferenceLinks = true,
                Filter = "Comma Seperated Variables (*.csv)|*.csv|All Files (*.*)|*.*",
                Multiselect = false,
                ShowReadOnly = false,
                Title = "Load Sensor Data",
                ValidateNames = true
            };

            var result = fileDialog.ShowDialog(this);

            if (result == true)
            {
                var fieldWidth = int.Parse(MapWidthTextBlock.Text);
                var fieldHeight = int.Parse(MapHeightTextBlock.Text);

                Cursor = Cursors.Wait;
                LoadMapButton.IsEnabled = false;
                ResetMapButton.IsEnabled = false;
                MapWidthTextBlock.IsEnabled = false;
                MapHeightTextBlock.IsEnabled = false;

                var worker = new Thread(LoadMap);
                worker.Start(new LoadMapData { FileName = fileDialog.FileName, MapSize = new Size(fieldWidth, fieldHeight) });
            }
        }

        private struct LoadMapData
        {
            public Size MapSize;
            public string FileName;
        }

        private void LoadMapComplete()
        {
            ResetMapButton.IsEnabled = true;
            LoadMapButton.IsEnabled = true;
            MapWidthTextBlock.IsEnabled = true;
            MapHeightTextBlock.IsEnabled = true;
            Cursor = Cursors.Arrow;
        }

        private void LoadMap(object fileName)
        {
            var data = (LoadMapData)fileName;

            field = new Field((int)data.MapSize.Width, (int)data.MapSize.Height, data.FileName);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(DrawMap));

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(LoadMapComplete));
        }

        private Bitmap map;

        private void DrawMap()
        {
            var picture = new Bitmap(field.Count * 10, field.First().Count * 10);
            var graphics = Graphics.FromImage(picture);

            lock (field)
            {
                var rects = field.AsParallel().SelectMany((row, y) => row.Select((square, x) => new
                {
                    Rectangle = new Rectangle(x * 10, y * 10, 10, 10),
                    Brush = new SolidBrush(square.SquareColor)
                }));

                foreach (var rect in rects)
                {
                    var r = new System.Windows.Shapes.Rectangle();
                    MapCanvas.Children.Add(r);
                    r.SetValue(Canvas.TopProperty, (double)(rect.Rectangle.Top * 10));
                    r.SetValue(Canvas.LeftProperty, (double)(rect.Rectangle.Left * 10));
                    r.Width = 10;
                    r.Height = 10;
                    r.Visibility = Visibility.Visible;
                }
            }

            //for (var y = 0; y < field.Count; y++)
            //{
            //    for (var x = 0; x < field[y].Count; x++)
            //    {
            //        var rect = new Rectangle(x * 10, y * 10, 10, 10);
            //        var color = field[y][x].SquareColor;
            //        graphics.FillRectangle(new SolidBrush(color), rect);
            //    }
            //}


            //map = new Bitmap(picture);

            //var img = CreateBitmapSource(picture);
            //img.Freeze();

            //Dispatcher.Invoke(new Action(() => DataContext = img));
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private static BitmapSource CreateBitmapSource(Bitmap bitmap)
        {

            var hBitmap = bitmap.GetHbitmap();

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var source = BitmapSource.Create(bitmap.Width, bitmap.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Pbgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmap.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return source;
        }

        private void UpdateMap()
        {
            var newPicture = new Bitmap(map);

            lock (field)
            {

            }
        }
    }
}
