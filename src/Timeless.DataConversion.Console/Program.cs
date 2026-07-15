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
        public static void Main(string[] args)
        {
            //Command line parsing library from http://cmdline.codeplex.com/
            var cmd = new ConsoleCmdLine();
            var xmlInputFilePath = new CmdLineString("xml", true, "XML file to convert.");
            var outputDirectory = new CmdLineString("dir", false, "Directory to save the CSV result to.");
            var outputZipFilePath = new CmdLineString("zip", false, "Optional zip file path to save the CSV result archive to.");
            var outputEncoding = new CmdLineString("encoding", false, "CSV output encoding. Defaults to unicode. Examples: unicode, utf-8.");

            cmd.RegisterParameter(xmlInputFilePath);
            cmd.RegisterParameter(outputDirectory);
            cmd.RegisterParameter(outputZipFilePath);
            cmd.RegisterParameter(outputEncoding);
            cmd.Parse(args);

            if ((!outputDirectory.Exists || string.IsNullOrEmpty(outputDirectory.Value)) &&
                (!outputZipFilePath.Exists || string.IsNullOrEmpty(outputZipFilePath.Value)))
            {
                throw new CmdLineException("Specify either -dir or -zip.");
            }

            Encoding encoding = ResolveOutputEncoding(outputEncoding);
            var service = new XmlPublicConversionService(new XmlConversionLimits());
            XmlConversionPreview preview = service.CreatePreview(xmlInputFilePath.Value);
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);

            if (outputZipFilePath.Exists && !string.IsNullOrEmpty(outputZipFilePath.Value))
            {
                service.ExportConfirmedConversionToZip(xmlInputFilePath.Value, outputZipFilePath.Value, encoding, preview, confirmation);
                return;
            }

            service.ExportConfirmedConversion(xmlInputFilePath.Value, outputDirectory.Value, encoding, preview, confirmation);
        }

        internal static string BuildCsvDestinationFilePath(string outputDirectory, string tableName)
        {
            return Path.Combine(outputDirectory, tableName + ".csv");
        }

        internal static Encoding ResolveOutputEncoding(CmdLineString outputEncoding)
        {
            if (!outputEncoding.Exists || string.IsNullOrEmpty(outputEncoding.Value))
            {
                return Encoding.Unicode;
            }

            return Encoding.GetEncoding(outputEncoding.Value);
        }
    }
}
