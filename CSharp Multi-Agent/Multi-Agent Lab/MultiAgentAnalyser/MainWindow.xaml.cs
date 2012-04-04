using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using MultiAgentLibrary;
using Visiblox.Charts;

namespace MultiAgentAnalyser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var reader = new XmlSerializer(typeof(SnapshotCollection));
            var data = reader.Deserialize(File.OpenRead(@"C:\Users\Tony\Documents\tr-computational-intelligence-research\CSharp Multi-Agent\Multi-Agent Lab\MultiAgentBatch\bin\Release\Cycle Count Analysis.xml")) as SnapshotCollection;

            var snaps = data.Snapshots.AsParallel().Where(s => s.RouteLength > 0);
            var snapsSummary = snaps.GroupBy(d => d.CycleCount).Select(s => new { s.Key, Average = s.Average(i => i.RouteLength)});

            var averageSeries = new DataSeries<int, double>();
            averageSeries.AddRange(snapsSummary.Select(s => new DataPoint<int, double>(s.Key, s.Average)).OrderBy(p => p.X));

            var allSeries = new DataSeries<int, int>();
            allSeries.AddRange(snaps.Distinct(new SnapshotComparer()).Select(s => new DataPoint<int, int>(s.CycleCount, s.RouteLength)));


            var averageLine = new LineSeries() { DataSeries = averageSeries };
            var allLine = new LineSeries() { DataSeries = allSeries, ShowLine = false, ShowPoints = true, PointSize = 1 };

            var graph = new Chart();
            graph.Series.Add(allLine);
            graph.Series.Add(averageLine);
            Content = graph;
        }
    }
}
