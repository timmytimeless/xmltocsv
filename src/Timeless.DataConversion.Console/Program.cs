using System.IO;
using System.Text;
using Timeless.DataConversion.XmlToCsvStrategy;

namespace Timeless.DataConversion.Console
{
    public class Program
    {
        /// <summary>
        /// XML to CSV Command Line Converter. User as follows: Timeless.DataConversion.Console.exe -xml C:\payslip.xml -dir C:\my_output
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

            var converter = new XmlToCsvUsingDataSet(xmlInputFilePath.Value);
            var context = new XmlToCsvContext(converter);

            foreach (string xmlTableName in context.Strategy.TableNameCollection)
            {
                string csvDestinationFilePath = Path.Combine(outputDirectory.Value, xmlTableName + ".csv");
                context.Execute(xmlTableName, csvDestinationFilePath, Encoding.Unicode);
            }
        }
    }
}
