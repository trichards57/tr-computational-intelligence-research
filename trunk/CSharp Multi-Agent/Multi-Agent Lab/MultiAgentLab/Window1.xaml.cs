using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using MultiAgentLibrary;

namespace MultiAgentLab
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Media;

    using Size = System.Windows.Size;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1
    {
        //readonly List<Agent> agentsList = new List<Agent>();

        private Timer agentTicker;

        private readonly BackgroundWorker agentWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
        Field field;

        public Window1()
        {
            InitializeComponent();
            agentWorker.DoWork += ProcessAgents;
        }

        public void ProcessAgents(object sender, DoWorkEventArgs args)
        {
            var count = 0;
            while (!agentWorker.CancellationPending)
            {
                field.CycleAgents();

                count++;
                if (count > 10 && field.AgentsList.Count < 250)
                {
                    var agent = new Agent(field.StartPoint);
                    lock (field.AgentsList)
                    {
                        field.AgentsList.Add(agent);
                    }
                    Dispatcher.Invoke(new Action(() => CreateAgentImage(agent)));
                    count = 0;
                }
            }
        }

        private void TimerTick(object state)
        {
            Dispatcher.Invoke(new Action(UpdateMap), null);
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

            field.AgentsList.Add(new Agent(field.StartPoint));

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(DrawMap));

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(LoadMapComplete));
        }

        private void UpdateMap()
        {
            foreach (var rect in MapCanvas.Children.OfType<System.Windows.Shapes.Rectangle>())
            {
                var r = (Rect)rect.Tag;
                rect.Fill = new SolidColorBrush(field[(int)(r.Y / 10)][(int)(r.X / 10)].SquareColor);
            }

            foreach (var agent in MapCanvas.Children.OfType<System.Windows.Shapes.Ellipse>())
            {
                var a = (Agent)agent.Tag;
                agent.SetValue(Canvas.TopProperty, a.Position.Y * 10);
                agent.SetValue(Canvas.LeftProperty, a.Position.X * 10);
            }
        }

        private void DrawMap()
        {
            MapCanvas.Width = field.Count * 10;
            MapCanvas.Height = field.First().Count * 10;

            var rects = field.AsParallel().SelectMany((row, y) => row.Select((square, x) => new
            {
                Rectangle = new Rect(x * 10, y * 10, 10, 10),
                square.SquareColor
            }));

            foreach (var rect in rects)
            {
                var r = new System.Windows.Shapes.Rectangle();
                MapCanvas.Children.Add(r);
                r.SetValue(Canvas.TopProperty, rect.Rectangle.Top);
                r.SetValue(Canvas.LeftProperty, rect.Rectangle.Left);
                r.Width = 10;
                r.Height = 10;
                r.Fill = new SolidColorBrush(rect.SquareColor);
                r.Tag = rect.Rectangle;
            }

            foreach (var agent in field.AgentsList)
            {
                CreateAgentImage(agent);
            }

        }

        private void CreateAgentImage(Agent agent)
        {
            var e = new System.Windows.Shapes.Ellipse();
            MapCanvas.Children.Add(e);
            e.SetValue(Canvas.TopProperty, agent.Position.Y * 10);
            e.SetValue(Canvas.LeftProperty, agent.Position.X * 10);
            e.Width = 10;
            e.Height = 10;
            e.Fill = Brushes.Magenta;
            e.Tag = agent;
        }

        private void ResetMapClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(ResetMap));
        }

        private void ResetMap()
        {
            lock (field)
            {
                foreach (var square in field.AsParallel().SelectMany(row => row))
                {
                    square.PheremoneLevel = 1.0;
                }
            }

            lock (field.AgentsList)
            {
                var l = MapCanvas.Children.OfType<System.Windows.Shapes.Ellipse>().ToList();
                foreach (var e in l)
                    MapCanvas.Children.Remove(e);
                field.AgentsList.Clear();
                var agentsCount = int.Parse(AgentCountTextBox.Text);
                for (var i = 0; i < agentsCount; i++)
                {
                    var agent = new Agent(field.StartPoint);
                    field.AgentsList.Add(agent);
                    CreateAgentImage(agent);
                }
            }

            Dispatcher.Invoke(new Action(UpdateMap));
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            var interval = int.Parse(UpdateRateTextBox.Text);
            if (agentTicker == null)
                agentTicker = new Timer(TimerTick, field, 0, interval);
            else
                agentTicker.Change(0, interval);

            agentWorker.RunWorkerAsync();
        }

        private void StopButtonClick(object sender, RoutedEventArgs e)
        {
            agentWorker.CancelAsync();
            agentTicker.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
