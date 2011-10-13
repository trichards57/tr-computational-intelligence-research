using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UsefulClasses;
using System.Reflection;
using UsefulClasses.Exceptions;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MultiAgentLibrary;

namespace MultiAgentBatch
{
    class Program
    {
        static ParameterManager parameterManager = new ParameterManager();

        static int Main(string[] args)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine("Multi-Agent Lab Console : {0}", assemblyVersion.ToString(3));
            Console.WriteLine("#############################################################################\n");
            if (args.Length < 1)
            {
                WriteInstructions();
                return 1;
            }

            var dataFileParameter = new Parameter<string>("d", null, s => s) { Description = "The filename of a CSV file containing sensor readings.", FriendlyName = "Sensor Data File", Required = true };
            var consolePathParameter = new Parameter<string>("ap", @"..\..\..\MultiAgentConsole\bin\Debug\MultiAgentConsole.exe", s => s) { Description = "The path to the console program.", FriendlyName = "Console Path" };
            var outputFileParameter = new Parameter<string>("of", "finalOutput.xml", s => s) { Description = "The filename of an XML file containing the experiment results.", FriendlyName = "Output File" };

            parameterManager.RegisterParameter(dataFileParameter);
            parameterManager.RegisterParameter(consolePathParameter);
            parameterManager.RegisterParameter(outputFileParameter);

            try
            {
                parameterManager.ProcessParameters(args);
            }
            catch (InvalidParameterException ex)
            {
                var str = string.Format(CultureInfo.CurrentCulture, "Unable to process command line arguments : {0}", ex.Message);
                var parts = str.Wrap(Console.WindowWidth);

                foreach (var p in parts)
                    Console.WriteLine(p);

                Console.WriteLine();
                WriteInstructions();
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

            Console.WriteLine(parameterManager.GenerateParameterStatusMessage());


            var snaps = new SnapshotCollection();

            for (var iteration = 0; iteration < 200; iteration++)
            {

                var processData = new ProcessStartInfo(consolePath);
                processData.Arguments = string.Format("/d:{0} /os:output.xml /bm:true /xi:1000 /c:50000", dataFile);

                var process = Process.Start(processData);

                process.WaitForExit();

                var reader = new XmlSerializer(typeof(SnapshotCollection));
                using (var f = File.OpenRead("output.xml"))
                {
                    var snapshots = reader.Deserialize(f) as SnapshotCollection;
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
            using (var outputStream = File.Open(outputFile, FileMode.Create, FileAccess.Write))
            {
                var writer = new XmlSerializer(typeof(SnapshotCollection));
                writer.Serialize(outputStream, snaps);
            }
            return 0;
        }

        static void WriteInstructions()
        {
            Console.WriteLine(parameterManager.GenerateCommandLineUsageMessage("MultiAgentBatch.exe"));

            Console.WriteLine("Press ENTER to exit...");

            Console.ReadLine();
        }
    }
}
