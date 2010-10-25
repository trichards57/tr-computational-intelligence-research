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
using MultiAgentLab.Classes;
using System.Threading;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Windows.Interop;

namespace MultiAgentLab
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        List<Agent> agentsList = new List<Agent>();
        Timer agentTicker;
        Timer newAgentTicker;
        int count = 0;
        Field field;

        public Window1()
        {
            InitializeComponent();
        }

        private void TimerTick(object state)
        {
            var field = state as Field;
            lock (agentsList)
            {
                foreach (var agent in agentsList)
                    agent.Process(field);
            }

            foreach (var row in field)
                foreach (var square in row)
                    if (square.PheremoneLevel > 1)
                        square.PheremoneLevel -= 0.00001;
            count++;
            if (count > 10000)
            {
                agentsList.Add(new Agent(field.StartPoint));
                count = 0;
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void LoadMapClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.AddExtension = false;
            fileDialog.CheckFileExists = true;
            fileDialog.CheckPathExists = true;
            fileDialog.DefaultExt = "csv";
            fileDialog.DereferenceLinks = true;
            fileDialog.Filter = "Comma Seperated Variables (*.csv)|*.csv|All Files (*.*)|*.*";
            fileDialog.Multiselect = false;
            fileDialog.ShowReadOnly = false;
            fileDialog.Title = "Load Sensor Data";
            fileDialog.ValidateNames = true;

            var result = fileDialog.ShowDialog(this);

            if (result == true)
            {
                var fieldWidth = int.Parse(MapWidthTextBlock.Text);
                var fieldHeight = int.Parse(MapHeightTextBlock.Text);

                Cursor = Cursors.Wait;
                LoadMapButton.IsEnabled = false;
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

            DrawMap();

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(LoadMapComplete));
        }

        private void DrawMap()
        {
            var picture = new System.Drawing.Bitmap(field.Count * 10, field.First().Count * 10);
            var graphics = System.Drawing.Graphics.FromImage(picture);

            lock (field)
            {
                for (var y = 0; y < field.Count; y++)
                {
                    for (var x = 0; x < field[y].Count; x++)
                    {
                        var rect = new System.Drawing.Rectangle(x * 10, y * 10, 10, 10);
                        var color = field[y][x].SquareColor;
                        graphics.FillRectangle(new System.Drawing.SolidBrush(color), rect);
                    }
                }
            }

            Dispatcher.Invoke(new Action<System.Drawing.Bitmap>(UpdateMap), picture);
        }

        private void UpdateMap(System.Drawing.Bitmap image)
        {
            DataContext = Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
