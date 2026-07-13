using System.IO;
using System.Text;
using Timeless.DataConversion.XmlToCsv;

namespace Timeless.DataConversion.Console
{
    public class Program
    {
        /// <summary>
        /// XML to CSV Command Line Converter. Use as follows: Timeless.DataConversion.Console.exe -xml C:\payslip.xml -dir C:\my_output
        /// </summary>
        /// <param name="args">First parameter selects the XML file, second parameter specifies the output directory.</param>
        static void Main(string[] args)
        {
            //Command line parsing library from http://cmdline.codeplex.com/
            var cmd = new ConsoleCmdLine();
            var xmlInputFilePath = new CmdLineString("xml", true, "XML file to convert.");
            var outputDirectory = new CmdLineString("dir", true, "Directory to save the CSV result to.");

            cmd.RegisterParameter(xmlInputFilePath);
            cmd.RegisterParameter(outputDirectory);
            cmd.Parse(args);

            using var converter = new XmlToCsvUsingDataSet(xmlInputFilePath.Value);
            
            foreach (string xmlTableName in converter.TableNameCollection)
            {
                var csvDestinationFilePath = BuildCsvDestinationFilePath(outputDirectory.Value, xmlTableName);
                
                converter.ExportToCsv(xmlTableName, csvDestinationFilePath, Encoding.Unicode);
            }
        }

        internal static string BuildCsvDestinationFilePath(string outputDirectory, string xmlTableName)
        {
            return Path.Combine(outputDirectory, xmlTableName + ".csv");
        }
    }
}
