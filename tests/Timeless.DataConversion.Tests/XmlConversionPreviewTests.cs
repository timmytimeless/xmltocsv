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
        public void CreateConversionPreviewTreatsASingleStructuredRecordAsOneRow()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<root><name>Only One</name></root>");

            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);

            XmlTablePreview table = preview.Tables.Single();
            Assert.That(table.Path, Is.EqualTo("/root"));
            Assert.That(table.RowCount, Is.EqualTo(1));
            Assert.That(table.Columns.Single().Path, Is.EqualTo("/root/name"));
            Assert.That(preview.Warnings, Has.None.Contains("No candidate tables"));
        }

        [Test]
        public void CreateConversionPreviewPreservesSingleRecordLeafAttributes()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<bookstore><book category=\"cooking\">" +
                "<title lang=\"en\">Everyday Italian</title>" +
                "<author>Giada De Laurentiis</author><year>2005</year><price>30.00</price>" +
                "</book></bookstore>");

            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            XmlTablePreview table = preview.Tables.Single();

            Assert.That(table.Path, Is.EqualTo("/bookstore/book"));
            Assert.That(table.RowCount, Is.EqualTo(1));
            Assert.That(table.Columns.Select(item => item.Path), Is.EquivalentTo(new[]
            {
                "/bookstore/book/@category",
                "/bookstore/book/title",
                "/bookstore/book/title/@lang",
                "/bookstore/book/author",
                "/bookstore/book/year",
                "/bookstore/book/price"
            }));
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
        public void ExportConfirmedConversionUsesConfirmedPlanAdjustments()
        {
            string xmlPath = WriteOrdersXml();
            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            XmlTablePlanConfirmation orderTable = confirmation.Tables.Single(item => item.Path == "/orders/order");
            XmlColumnPlanConfirmation idColumn = orderTable.Columns.Single(item => item.Path == "/orders/order/@id");
            string outputDirectory = CreateOutputDirectory();

            orderTable.Name = "orders";
            idColumn.Name = "order_id";

            XmlToCsvConverter.ExportConfirmedConversion(xmlPath, outputDirectory, Encoding.UTF8, preview, confirmation);

            string csv = File.ReadLines(Path.Combine(outputDirectory, "orders.csv"), Encoding.UTF8).First();
            Assert.That(csv, Is.EqualTo("_row_id,order_id,number"));
        }

        [Test]
        public void ExportConfirmedConversionStreamsXmlAgainAfterPreviewAndConfirmation()
        {
            string xmlPath = WriteOrdersXml();
            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            string outputDirectory = CreateOutputDirectory();

            File.WriteAllText(xmlPath,
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"200\"><number>200</number><items><item sku=\"C\"><quantity>4</quantity></item></items></order>" +
                "<order id=\"201\"><number>201</number><items><item sku=\"D\"><quantity>5</quantity></item></items></order>" +
                "<order id=\"202\"><number>202</number><items><item sku=\"E\"><quantity>6</quantity></item></items></order>" +
                "</orders>",
                Encoding.UTF8);

            XmlToCsvConverter.ExportConfirmedConversion(xmlPath, outputDirectory, Encoding.UTF8, preview, confirmation);

            string orderCsv = File.ReadAllText(Path.Combine(outputDirectory, "orders_order.csv"), Encoding.UTF8);
            Assert.That(orderCsv, Is.EqualTo(
                "_row_id,id,number" + System.Environment.NewLine +
                "\"1\",\"200\",\"200\"" + System.Environment.NewLine +
                "\"2\",\"201\",\"201\"" + System.Environment.NewLine +
                "\"3\",\"202\",\"202\"" + System.Environment.NewLine));
        }

        [Test]
        public void CreateConfirmedConversionSampleMatchesExportedValuesAndRelationships()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><number>Line 1\nLine 2</number><items><item sku=\"A,&quot;B&quot;\"><quantity>2</quantity></item><item sku=\"C\"><quantity>3</quantity></item></items></order>" +
                "<order id=\"101\"><number>101</number><items><item sku=\"D\"><quantity>4</quantity></item></items></order>" +
                "</orders>");
            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);

            XmlConversionSample sample = XmlToCsvConverter.CreateConfirmedConversionSample(
                xmlPath,
                preview,
                confirmation,
                2,
                500,
                new XmlConversionLimits());

            XmlTableSample orders = sample.Tables.Single(table => table.Path == "/orders/order");
            XmlTableSample items = sample.Tables.Single(table => table.Path == "/orders/order/items/item");

            Assert.That(orders.Columns, Is.EqualTo(new[] { "_row_id", "id", "number" }));
            Assert.That(orders.Rows.Select(row => row.Values.Select(value => value.Value)), Is.EqualTo(new[]
            {
                new[] { "1", "100", "Line 1\\nLine 2" },
                new[] { "2", "101", "101" }
            }));
            Assert.That(orders.IsTruncated, Is.False);

            Assert.That(items.Columns, Is.EqualTo(new[] { "_row_id", "_parent_row_id", "sku", "quantity" }));
            Assert.That(items.Rows.Select(row => row.Values.Select(value => value.Value)), Is.EqualTo(new[]
            {
                new[] { "1", "1", "A,\"B\"", "2" },
                new[] { "2", "1", "C", "3" }
            }));
            Assert.That(items.IsTruncated, Is.True);
        }

        [Test]
        public void CreateConfirmedConversionSampleUsesConfirmedNamesAndRejectsInvalidRowLimit()
        {
            string xmlPath = WriteOrdersXml();
            XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            XmlTablePlanConfirmation orderTable = confirmation.Tables.Single(table => table.Path == "/orders/order");
            orderTable.Name = "orders";
            orderTable.Columns.Single(column => column.Path == "/orders/order/@id").Name = "order_id";

            XmlConversionSample sample = XmlToCsvConverter.CreateConfirmedConversionSample(
                xmlPath,
                preview,
                confirmation,
                1,
                500,
                new XmlConversionLimits());

            XmlTableSample orders = sample.Tables.Single(table => table.Path == "/orders/order");
            Assert.That(orders.Name, Is.EqualTo("orders"));
            Assert.That(orders.Columns, Is.EqualTo(new[] { "_row_id", "order_id", "number" }));
            Assert.That(orders.Rows, Has.Count.EqualTo(1));

            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                XmlToCsvConverter.CreateConfirmedConversionSample(
                    xmlPath,
                    preview,
                    confirmation,
                    0,
                    500,
                    new XmlConversionLimits()));

            XmlConversionSample truncatedSample = XmlToCsvConverter.CreateConfirmedConversionSample(
                xmlPath,
                preview,
                confirmation,
                1,
                2,
                new XmlConversionLimits());
            XmlSampleValue truncatedOrderId = truncatedSample.Tables
                .Single(table => table.Path == "/orders/order")
                .Rows[0]
                .Values[1];
            Assert.That(truncatedOrderId.Value, Is.EqualTo("10"));
            Assert.That(truncatedOrderId.IsTruncated, Is.True);
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
