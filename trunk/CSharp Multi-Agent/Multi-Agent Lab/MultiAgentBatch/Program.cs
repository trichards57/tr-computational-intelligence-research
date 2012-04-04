using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using MultiAgentLibrary;
using UsefulClasses;
using UsefulClasses.Exceptions;

namespace MultiAgentBatch
{
    class Program
    {
        static readonly ParameterManager ParameterManager = new ParameterManager();

        static int Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Multi-Agent Lab Console : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");

            var dataFileParameter = new Parameter<string>("d", null, s => s) { Description = "The filename of a CSV file containing sensor readings.", FriendlyName = "Sensor Data File", Required = true };
            var consolePathParameter = new Parameter<string>("ap", @"..\..\..\MultiAgentConsole\bin\Debug\MultiAgentConsole.exe", s => s) { Description = "The path to the console program.", FriendlyName = "Console Path" };
            var outputFileParameter = new Parameter<string>("of", "finalOutput.xml", s => s) { Description = "The filename of an XML file containing the experiment results.", FriendlyName = "Output File" };

            ParameterManager.RegisterParameter(dataFileParameter);
            ParameterManager.RegisterParameter(consolePathParameter);
            ParameterManager.RegisterParameter(outputFileParameter);

            try
            {
                ParameterManager.ProcessParameters(args);
            }
            catch (InvalidParameterException ex)
            {
                var str = string.Format(CultureInfo.CurrentCulture, "Unable to process command line arguments : {0}", ex.Message);
                var parts = str.Wrap(Console.WindowWidth);

                foreach (var p in parts)
                    Console.WriteLine(p);

                Console.WriteLine();
                WriteInstructions();
                Console.ReadLine();
                return 1;
            }

            var dataFile = dataFileParameter.Value;
            var consolePath = consolePathParameter.Value;
            var outputFile = outputFileParameter.Value;

            if (!File.Exists(dataFile))
            {
                Console.WriteLine("Data file {0} does not exist.", dataFile);
                return 2;
            }

            if (!File.Exists(consolePath))
            {
                Console.WriteLine("Console program {0} does not exist.", consolePath);
                return 2;
            }

            Console.WriteLine(ParameterManager.GenerateParameterStatusMessage());


            var snaps = new SnapshotCollection();

            for (var agentCount = 100; agentCount < 2000; agentCount += 100)
            {
                for (var iteration = 0; iteration < 200; iteration++)
                {

                    var processData = new ProcessStartInfo(consolePath)
                                          {
                                              Arguments =
                                                  string.Format("/d:{0} /os:output.xml /bm:true /ma:{1} /xi:1000 /c:20000", dataFile, agentCount)
                                          };

                    var process = Process.Start(processData);

                    process.WaitForExit();

                    var reader = new XmlSerializer(typeof(SnapshotCollection));
                    using (var f = File.OpenRead("output.xml"))
                    {
                        var snapshots = reader.Deserialize(f) as SnapshotCollection;
                        Debug.Assert(snapshots != null, "Snapshots must deserialize properly.");
                        snaps.Snapshots.AddRange(snapshots.Snapshots);
                    }

                    //using (var file = File.Open("output.xml", FileMode.Open, FileAccess.Read))
                    //{
                    //    var doc = new XmlDocument();
                    //    doc.Load(file);
                    //    var child = doc.DocumentElement.FirstChild;
                    //    outputWriter.WriteRaw(child.OuterXml);
                    //}

                    Console.WriteLine("Iteration {0} complete.", iteration);
                }
            }
            using (var outputStream = File.Open(outputFile, FileMode.Create, FileAccess.Write))
            {
                var writer = new XmlSerializer(typeof(SnapshotCollection));
                writer.Serialize(outputStream, snaps);
            }
            return 0;
        }

        static void WriteInstructions()
        {
            Console.WriteLine(ParameterManager.GenerateCommandLineUsageMessage("MultiAgentBatch.exe"));

            Console.WriteLine("Press ENTER to exit...");

            Console.ReadLine();
        }
    }
}
