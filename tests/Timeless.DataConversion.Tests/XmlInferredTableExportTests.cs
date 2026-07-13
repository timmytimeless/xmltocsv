using System;
using System.IO;
using System.Text;
using Timeless.DataConversion.XmlToCsv;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    public class XmlInferredTableExportTests
    {
        [Test]
        public void ExportInferredTablesWritesOneCsvPerInferredTable()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><number>100</number><items><item sku=\"A\"><quantity>2</quantity></item><item sku=\"B\"><quantity>3</quantity></item></items></order>" +
                "<order id=\"101\"><number>101</number><items><item sku=\"C\"><quantity>4</quantity></item></items></order>" +
                "</orders>");
            string outputDirectory = CreateOutputDirectory();

            XmlToCsvConverter.ExportInferredTables(xmlPath, outputDirectory, Encoding.UTF8);

            Assert.That(File.Exists(Path.Combine(outputDirectory, "orders_order.csv")), Is.True);
            Assert.That(File.Exists(Path.Combine(outputDirectory, "items_item.csv")), Is.True);
        }

        [Test]
        public void ExportInferredTablesWritesGeneratedRowIdsAndParentIds()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><number>100</number><items><item sku=\"A\"><quantity>2</quantity></item><item sku=\"B\"><quantity>3</quantity></item></items></order>" +
                "<order id=\"101\"><number>101</number><items><item sku=\"C\"><quantity>4</quantity></item></items></order>" +
                "</orders>");
            string outputDirectory = CreateOutputDirectory();

            XmlToCsvConverter.ExportInferredTables(xmlPath, outputDirectory, Encoding.UTF8);

            string orderCsv = File.ReadAllText(Path.Combine(outputDirectory, "orders_order.csv"), Encoding.UTF8);
            string itemCsv = File.ReadAllText(Path.Combine(outputDirectory, "items_item.csv"), Encoding.UTF8);

            Assert.That(orderCsv, Is.EqualTo(
                "_row_id,id,number" + Environment.NewLine +
                "\"1\",\"100\",\"100\"" + Environment.NewLine +
                "\"2\",\"101\",\"101\"" + Environment.NewLine));
            Assert.That(itemCsv, Is.EqualTo(
                "_row_id,_parent_row_id,sku,quantity" + Environment.NewLine +
                "\"1\",\"1\",\"A\",\"2\"" + Environment.NewLine +
                "\"2\",\"1\",\"B\",\"3\"" + Environment.NewLine +
                "\"3\",\"2\",\"C\",\"4\"" + Environment.NewLine));
        }

        [Test]
        public void ExportInferredTablesEscapesCsvValues()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<rows>" +
                "<row><name>Hotel \"massandra\", Yalta</name></row>" +
                "<row><name>Line 1\nLine 2</name></row>" +
                "</rows>");
            string outputDirectory = CreateOutputDirectory();

            XmlToCsvConverter.ExportInferredTables(xmlPath, outputDirectory, Encoding.UTF8);

            string csv = File.ReadAllText(Path.Combine(outputDirectory, "rows_row.csv"), Encoding.UTF8);

            Assert.That(csv, Is.EqualTo(
                "_row_id,name" + Environment.NewLine +
                "\"1\",\"Hotel \"\"massandra\"\", Yalta\"" + Environment.NewLine +
                "\"2\",\"Line 1\\nLine 2\"" + Environment.NewLine));
        }

        [Test]
        public void ExportInferredTablesUsesExactChildTablePath()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order><number>100</number><items><item><sku>A</sku></item></items><notes><item><text>Ignore</text></item></notes></order>" +
                "<order><number>101</number><items><item><sku>B</sku></item></items><notes><item><text>Ignore</text></item></notes></order>" +
                "</orders>");
            string outputDirectory = CreateOutputDirectory();

            XmlToCsvConverter.ExportInferredTables(xmlPath, outputDirectory, Encoding.UTF8);

            string itemCsv = File.ReadAllText(Path.Combine(outputDirectory, "items_item.csv"), Encoding.UTF8);

            Assert.That(itemCsv, Is.EqualTo(
                "_row_id,_parent_row_id,sku" + Environment.NewLine +
                "\"1\",\"1\",\"A\"" + Environment.NewLine +
                "\"2\",\"2\",\"B\"" + Environment.NewLine));
        }

        private static string WriteTempXml(string xml)
        {
            string xmlPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".xml");
            File.WriteAllText(xmlPath, xml, Encoding.UTF8);
            return xmlPath;
        }

        private static string CreateOutputDirectory()
        {
            string outputDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(outputDirectory);
            return outputDirectory;
        }
    }
}
