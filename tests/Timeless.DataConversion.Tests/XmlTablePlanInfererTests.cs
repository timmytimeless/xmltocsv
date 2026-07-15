using System.IO;
using System.Linq;
using System.Text;
using Timeless.DataConversion.XmlToCsv;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    public class XmlTablePlanInfererTests
    {
        [Test]
        public void InferTablesDetectsRepeatedRowPaths()
        {
            XmlInferredTablePlan plan = InferPlan(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><number>100</number><customer>Alpha</customer></order>" +
                "<order id=\"101\"><number>101</number><customer>Beta</customer></order>" +
                "</orders>");

            XmlInferredTable orderTable = plan.Tables.Single(item => item.Path == "/orders/order");

            Assert.That(orderTable.Name, Is.EqualTo("orders_order"));
            Assert.That(orderTable.RowCount, Is.EqualTo(2));
            Assert.That(orderTable.Columns.Select(item => item.Path), Does.Contain("/orders/order/@id"));
            Assert.That(orderTable.Columns.Select(item => item.Path), Does.Contain("/orders/order/customer"));
            Assert.That(orderTable.Columns.Select(item => item.Path), Does.Contain("/orders/order/number"));
            Assert.That(orderTable.Reasons, Does.Contain("repeated sibling element"));
            Assert.That(orderTable.Reasons, Has.Some.Contains("stable structure"));
        }

        [Test]
        public void InferTablesDetectsNestedChildTables()
        {
            XmlInferredTablePlan plan = InferPlan(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order><number>100</number><items><item><sku>A</sku><quantity>2</quantity></item><item><sku>B</sku><quantity>3</quantity></item></items></order>" +
                "<order><number>101</number><items><item><sku>C</sku><quantity>4</quantity></item></items></order>" +
                "</orders>");

            XmlInferredTable orderTable = plan.Tables.Single(item => item.Path == "/orders/order");
            XmlInferredTable itemTable = plan.Tables.Single(item => item.Path == "/orders/order/items/item");

            Assert.That(orderTable.ChildTables.Select(item => item.Path), Does.Contain(itemTable.Path));
            Assert.That(itemTable.Name, Is.EqualTo("items_item"));
            Assert.That(itemTable.RowCount, Is.EqualTo(3));
            Assert.That(itemTable.Columns.Select(item => item.Name), Does.Contain("quantity"));
            Assert.That(itemTable.Columns.Select(item => item.Name), Does.Contain("sku"));
        }

        [Test]
        public void InferTablesCarriesColumnTypeHints()
        {
            XmlInferredTablePlan plan = InferPlan(
                "<?xml version=\"1.0\"?>" +
                "<rows>" +
                "<row id=\"1\"><amount>10.25</amount><active>true</active><name>Alpha</name></row>" +
                "<row id=\"2\"><amount>11.50</amount><active>false</active><name>Beta</name></row>" +
                "</rows>");

            XmlInferredTable table = plan.Tables.Single(item => item.Path == "/rows/row");

            Assert.That(table.Columns.Single(item => item.Path == "/rows/row/@id").TypeHint, Is.EqualTo("integer"));
            Assert.That(table.Columns.Single(item => item.Path == "/rows/row/amount").TypeHint, Is.EqualTo("decimal"));
            Assert.That(table.Columns.Single(item => item.Path == "/rows/row/active").TypeHint, Is.EqualTo("boolean"));
            Assert.That(table.Columns.Single(item => item.Path == "/rows/row/name").TypeHint, Is.EqualTo("string"));
        }

        [Test]
        public void InferTablesRanksLeafHeavyRepeatedRowsAboveSingleColumnRows()
        {
            XmlInferredTablePlan plan = InferPlan(
                "<?xml version=\"1.0\"?>" +
                "<root>" +
                "<row><id>1</id><name>A</name><city>X</city></row>" +
                "<row><id>2</id><name>B</name><city>Y</city></row>" +
                "<tag><value>A</value></tag>" +
                "<tag><value>B</value></tag>" +
                "</root>");

            Assert.That(plan.Tables.First().Path, Is.EqualTo("/root/row"));
            Assert.That(plan.Tables.First().Score, Is.GreaterThan(plan.Tables.Single(item => item.Path == "/root/tag").Score));
        }

        private static XmlInferredTablePlan InferPlan(string xml)
        {
            string xmlPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".xml");
            File.WriteAllText(xmlPath, xml, Encoding.UTF8);

            XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlPath);
            return new XmlTablePlanInferer().InferTables(profile);
        }
    }
}
