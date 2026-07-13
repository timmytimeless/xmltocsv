using System.IO;
using System.Linq;
using System.Text;
using Timeless.DataConversion.XmlToCsv;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    public class XmlConversionPreviewTests
    {
        [Test]
        public void CreateConversionPreviewReturnsTablesColumnsChildTablesAndWarnings()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><number>100</number><items><item sku=\"A\"><quantity>2</quantity></item></items></order>" +
                "<order id=\"101\"><number>101</number><items><item sku=\"B\"><quantity>3</quantity></item></items></order>" +
                "</orders>");

            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);

            XmlTablePreview orderTable = preview.Tables.Single(item => item.Path == "/orders/order");
            XmlTablePreview itemTable = preview.Tables.Single(item => item.Path == "/orders/order/items/item");

            Assert.That(orderTable.Name, Is.EqualTo("orders_order"));
            Assert.That(orderTable.RowCount, Is.EqualTo(2));
            Assert.That(orderTable.Columns.Select(item => item.Path), Does.Contain("/orders/order/@id"));
            Assert.That(orderTable.ChildTablePaths, Does.Contain(itemTable.Path));
            Assert.That(itemTable.Columns.Single(item => item.Path == "/orders/order/items/item/quantity").TypeHint, Is.EqualTo("integer"));
            Assert.That(preview.Warnings, Has.Some.Contains("items_item").And.Contains("low inference score"));
        }

        [Test]
        public void CreateConversionPreviewWarnsWhenNoTablesAreInferred()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<root><name>Only One</name></root>");

            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);

            Assert.That(preview.Warnings, Has.Some.Contains("No candidate tables"));
        }

        [Test]
        public void ConfirmConversionPlanCanRenameTablesAndColumns()
        {
            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(WriteOrdersXml());
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            XmlTablePlanConfirmation orderTable = confirmation.Tables.Single(item => item.Path == "/orders/order");
            XmlColumnPlanConfirmation idColumn = orderTable.Columns.Single(item => item.Path == "/orders/order/@id");

            orderTable.Name = "orders";
            idColumn.Name = "order_id";

            XmlInferredTablePlan confirmedPlan = XmlToCsvConverter.ConfirmConversionPlan(preview, confirmation);

            XmlInferredTable confirmedOrderTable = confirmedPlan.Tables.Single(item => item.Path == "/orders/order");
            Assert.That(confirmedOrderTable.Name, Is.EqualTo("orders"));
            Assert.That(confirmedOrderTable.Columns.Single(item => item.Path == "/orders/order/@id").Name, Is.EqualTo("order_id"));
        }

        [Test]
        public void ConfirmConversionPlanCanExcludeTablesAndColumns()
        {
            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(WriteOrdersXml());
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            XmlTablePlanConfirmation itemTable = confirmation.Tables.Single(item => item.Path == "/orders/order/items/item");
            XmlTablePlanConfirmation orderTable = confirmation.Tables.Single(item => item.Path == "/orders/order");
            XmlColumnPlanConfirmation numberColumn = orderTable.Columns.Single(item => item.Path == "/orders/order/number");

            itemTable.Include = false;
            numberColumn.Include = false;

            XmlInferredTablePlan confirmedPlan = XmlToCsvConverter.ConfirmConversionPlan(preview, confirmation);

            XmlInferredTable confirmedOrderTable = confirmedPlan.Tables.Single(item => item.Path == "/orders/order");
            Assert.That(confirmedPlan.Tables.Select(item => item.Path), Does.Not.Contain("/orders/order/items/item"));
            Assert.That(confirmedOrderTable.ChildTables, Is.Empty);
            Assert.That(confirmedOrderTable.Columns.Select(item => item.Path), Does.Not.Contain("/orders/order/number"));
        }

        [Test]
        public void ExportInferredTablesUsesConfirmedPlanAdjustments()
        {
            string xmlPath = WriteOrdersXml();
            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            XmlTablePlanConfirmation orderTable = confirmation.Tables.Single(item => item.Path == "/orders/order");
            XmlColumnPlanConfirmation idColumn = orderTable.Columns.Single(item => item.Path == "/orders/order/@id");
            string outputDirectory = CreateOutputDirectory();

            orderTable.Name = "orders";
            idColumn.Name = "order_id";

            XmlInferredTablePlan confirmedPlan = XmlToCsvConverter.ConfirmConversionPlan(preview, confirmation);
            XmlToCsvConverter.ExportInferredTables(xmlPath, outputDirectory, Encoding.UTF8, confirmedPlan);

            string csv = File.ReadLines(Path.Combine(outputDirectory, "orders.csv"), Encoding.UTF8).First();
            Assert.That(csv, Is.EqualTo("_row_id,order_id,number"));
        }

        private static string WriteOrdersXml()
        {
            return WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><number>100</number><items><item sku=\"A\"><quantity>2</quantity></item></items></order>" +
                "<order id=\"101\"><number>101</number><items><item sku=\"B\"><quantity>3</quantity></item></items></order>" +
                "</orders>");
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
