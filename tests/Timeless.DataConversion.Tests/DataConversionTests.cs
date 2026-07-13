using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using Timeless.DataConversion.XmlToCsv;
using ConsoleProgram = Timeless.DataConversion.Console.Program;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    public class DataConversionTests
    {
        [Test]
        public void DataSetImplementationTest()
        {
            const string path = @"TestData/data.xml";
            using var converter = new XmlToCsvConverter(path);
            Assert.That(converter.TableNames.Count, Is.EqualTo(1));
            foreach (var tableName in converter.TableNames)
            {
                converter.Export(tableName, @"dataDataSet_" + tableName + ".csv", Encoding.Default);
            }
        }

        [Test]
        public void CanReadAttributeTest()
        {
            const string path = @"TestData/DataWithAttributes.xml";
            using var converter = new XmlToCsvConverter(path);
            Assert.That(converter.TableNames.Count, Is.EqualTo(2));

            foreach (var tableName in converter.TableNames)
            {
                converter.Export(tableName, @"TestData/dataDataSet_" + tableName + ".csv", Encoding.Default);
            }
        }

        [Test]
        public void DoubleQuoteEscapeTest()
        {
            const string path = @"TestData/DoubleQuoteEscape.xml";
            using var converter = new XmlToCsvConverter(path);
            Assert.That(converter.TableNames.Count, Is.EqualTo(1));

            foreach (var tableName in converter.TableNames)
            {
                converter.Export(tableName, string.Format(@"TestData/dataDataSet_{0}.csv", tableName), Encoding.Default);
            }
        }

        [Test]
        public void XmlSchemaNestedTableExceptionTest()
        {
            const string path = @"TestData/NestedDataError.xml";
            TestHelper.Throws<XmlSchemaException>(() => new XmlToCsvConverter(path),
                "Type 'DCLG-HIP:Inspector' is not declared.");
        }

        [Test]
        public void DuplicateNameErrorTest()
        {
            const string path = @"TestData/ErrorDuplicateName.xml";
            TestHelper.Throws<DuplicateNameException>(() => new XmlToCsvConverter(path), "A column named 'Employees' already belongs to this DataTable: cannot set a nested table name to the same name.");
        }

        [Test]
        public void DuplicateNameRenamedTest()
        {
            const string path = @"TestData/ErrorDuplicateName.xml";
            using var converter = new XmlToCsvConverter(path, true);
            Assert.That(converter.TableNames.Count, Is.EqualTo(2));

            foreach (var tableName in converter.TableNames)
            {
                converter.Export(tableName, @"ErrorDuplicateName_" + tableName + ".csv", Encoding.Default);
            }
        }

        [Test]
        public void ConsoleOutputPathUsesPlatformDirectorySeparator()
        {
            string outputDirectory = Path.Combine("tmp", "csv-output");

            string destinationFilePath = ConsoleProgram.BuildCsvDestinationFilePath(outputDirectory, "Employees");

            Assert.That(destinationFilePath, Is.EqualTo(outputDirectory + Path.DirectorySeparatorChar + "Employees.csv"));
        }

        [Test]
        public void ConsoleOutputEncodingDefaultsToUnicode()
        {
            var outputEncoding = new Timeless.DataConversion.Console.CmdLineString("encoding", false, "CSV output encoding.");

            Encoding encoding = ConsoleProgram.ResolveOutputEncoding(outputEncoding);

            Assert.That(encoding.WebName, Is.EqualTo(Encoding.Unicode.WebName));
        }

        [Test]
        public void ConsoleOutputEncodingCanBeConfigured()
        {
            var outputEncoding = new Timeless.DataConversion.Console.CmdLineString("encoding", false, "CSV output encoding.");
            outputEncoding.SetValue("utf-8");

            Encoding encoding = ConsoleProgram.ResolveOutputEncoding(outputEncoding);

            Assert.That(encoding.WebName, Is.EqualTo(Encoding.UTF8.WebName));
        }

        [Test]
        public void DataSetExportWritesExpectedCsvContents()
        {
            string csvPath = ConvertSingleTableToCsv(@"TestData/data.xml", "csvRow", Encoding.UTF8);

            string csv = File.ReadAllText(csvPath, Encoding.UTF8);

            Assert.That(csv, Is.EqualTo(
                "ZIPCode,CityName,StateAbbr,Country" + Environment.NewLine +
                "\"10001\",\"James Town\",\"NY\",\"USA\"" + Environment.NewLine +
                "\"10002\",\"Abbrevich\",\"CA\",\"USA\"" + Environment.NewLine +
                "\"10003\",\"Sommerville\",\"WY\",\"USA\"" + Environment.NewLine +
                "\"10004\",\"Loveland\",\"TX\",\"USA\"" + Environment.NewLine));
        }

        [Test]
        public void StreamingExportWritesExpectedCsvContents()
        {
            string csvPath = ConvertSingleTableToCsvUsingStreaming(@"TestData/data.xml", "csvRow", Encoding.UTF8);

            string csv = File.ReadAllText(csvPath, Encoding.UTF8);

            Assert.That(csv, Is.EqualTo(
                "ZIPCode,CityName,StateAbbr,Country" + Environment.NewLine +
                "\"10001\",\"James Town\",\"NY\",\"USA\"" + Environment.NewLine +
                "\"10002\",\"Abbrevich\",\"CA\",\"USA\"" + Environment.NewLine +
                "\"10003\",\"Sommerville\",\"WY\",\"USA\"" + Environment.NewLine +
                "\"10004\",\"Loveland\",\"TX\",\"USA\"" + Environment.NewLine));
        }

        [Test]
        public void StreamingExportFallsBackForUnsupportedTableShapes()
        {
            string csvPath = ConvertSingleTableToCsvUsingStreaming(@"TestData/DataWithAttributes.xml", "csvRow", Encoding.UTF8);

            string header = File.ReadLines(csvPath).First();

            Assert.That(header, Does.Contain("attribute"));
            Assert.That(header, Does.Contain("CityName"));
        }

        [Test]
        public void StreamingExportWritesBlankFieldsForMissingValues()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?><root><row><Name>First</Name><City>Yalta</City></row><row><Name>Second</Name></row></root>");

            string csvPath = ConvertSingleTableToCsvUsingStreaming(xmlPath, "row", Encoding.UTF8);

            string csv = File.ReadAllText(csvPath, Encoding.UTF8);
            Assert.That(csv, Is.EqualTo(
                "Name,City" + Environment.NewLine +
                "\"First\",\"Yalta\"" + Environment.NewLine +
                "\"Second\",\"\"" + Environment.NewLine));
        }

        [Test]
        public void DataSetExportEscapesQuotesAndCommas()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?><root><row><Name>Hotel \"massandra\", Yalta</Name></row></root>");

            string csvPath = ConvertSingleTableToCsv(xmlPath, "row", Encoding.UTF8);

            string csv = File.ReadAllText(csvPath, Encoding.UTF8);
            Assert.That(csv, Is.EqualTo(
                "Name" + Environment.NewLine +
                "\"Hotel \"\"massandra\"\", Yalta\"" + Environment.NewLine));
        }

        [Test]
        public void DataSetExportEscapesHeaders()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?><root><row><Name>Yalta</Name></row></root>");
            string csvPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".csv");

            using (var converter = new XmlToCsvConverter(xmlPath))
            {
                converter.DataSet.Tables["row"].Columns["Name"].ColumnName = "Display,Name";
                converter.Export("row", csvPath, Encoding.UTF8);
            }

            string header = File.ReadLines(csvPath).First();
            Assert.That(header, Is.EqualTo("\"Display,Name\""));
        }

        [Test]
        public void DataSetExportQuotesNonStringValues()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<root>" +
                "<xs:schema xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
                "<xs:element name=\"root\">" +
                "<xs:complexType><xs:sequence>" +
                "<xs:element name=\"row\" maxOccurs=\"unbounded\">" +
                "<xs:complexType><xs:sequence>" +
                "<xs:element name=\"Amount\" type=\"xs:int\" />" +
                "</xs:sequence></xs:complexType>" +
                "</xs:element>" +
                "</xs:sequence></xs:complexType>" +
                "</xs:element>" +
                "</xs:schema>" +
                "<row><Amount>42</Amount></row>" +
                "</root>");

            string csvPath = ConvertSingleTableToCsv(xmlPath, "row", Encoding.UTF8);

            string csv = File.ReadAllText(csvPath, Encoding.UTF8);
            Assert.That(csv, Is.EqualTo(
                "Amount" + Environment.NewLine +
                "\"42\"" + Environment.NewLine));
        }

        [Test]
        public void DataSetExportWritesExpectedHeaders()
        {
            string csvPath = ConvertSingleTableToCsv(@"TestData/data.xml", "csvRow", Encoding.UTF8);

            string header = File.ReadLines(csvPath).First();

            Assert.That(header, Is.EqualTo("ZIPCode,CityName,StateAbbr,Country"));
        }

        [Test]
        public void DataSetExportPreservesRowOrdering()
        {
            string csvPath = ConvertSingleTableToCsv(@"TestData/data.xml", "csvRow", Encoding.UTF8);

            string[] lines = File.ReadAllLines(csvPath, Encoding.UTF8);

            Assert.That(lines[1], Is.EqualTo("\"10001\",\"James Town\",\"NY\",\"USA\""));
            Assert.That(lines[2], Is.EqualTo("\"10002\",\"Abbrevich\",\"CA\",\"USA\""));
            Assert.That(lines[3], Is.EqualTo("\"10003\",\"Sommerville\",\"WY\",\"USA\""));
            Assert.That(lines[4], Is.EqualTo("\"10004\",\"Loveland\",\"TX\",\"USA\""));
        }

        [Test]
        public void DataSetExportUsesRequestedEncoding()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><root><row><City>München</City></row></root>");

            string csvPath = ConvertSingleTableToCsv(xmlPath, "row", Encoding.Unicode);

            byte[] bytes = File.ReadAllBytes(csvPath);
            string csv = Encoding.Unicode.GetString(bytes);

            Assert.That(bytes[0], Is.EqualTo(0xFF));
            Assert.That(bytes[1], Is.EqualTo(0xFE));
            Assert.That(csv, Does.Contain("\"München\""));
        }

        [Test]
        public void DataSetExportReplacesNewlinesInsideValues()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?><root><row><Notes>first" + Environment.NewLine + "second</Notes></row></root>");

            string csvPath = ConvertSingleTableToCsv(xmlPath, "row", Encoding.UTF8);

            string csv = File.ReadAllText(csvPath, Encoding.UTF8);
            Assert.That(csv, Is.EqualTo(
                "Notes" + Environment.NewLine +
                "\"first\\nsecond\"" + Environment.NewLine));
        }

        private static string ConvertSingleTableToCsv(string xmlPath, string tableName, Encoding encoding)
        {
            string csvPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".csv");
            using (var converter = new XmlToCsvConverter(xmlPath))
            {
                converter.Export(tableName, csvPath, encoding);
            }

            return csvPath;
        }

        private static string ConvertSingleTableToCsvUsingStreaming(string xmlPath, string tableName, Encoding encoding)
        {
            string csvPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".csv");
            using (var converter = XmlToCsvConverter.CreateStreaming(xmlPath))
            {
                converter.Export(tableName, csvPath, encoding);
            }

            return csvPath;
        }

        private static string WriteTempXml(string xml)
        {
            string xmlPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".xml");
            File.WriteAllText(xmlPath, xml, Encoding.UTF8);
            return xmlPath;
        }
    }
}
