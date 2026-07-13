using System.IO;
using System.Linq;
using System.Text;
using Timeless.DataConversion.XmlToCsv;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    public class XmlStructuralProfilerTests
    {
        [Test]
        public void ProfileCollectsElementAndAttributePaths()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><customer>Alpha</customer><total>42.50</total></order>" +
                "<order id=\"101\"><customer>Beta</customer><total>51</total></order>" +
                "</orders>");

            XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlPath);

            Assert.That(profile.ElementPaths.Keys, Does.Contain("/orders"));
            Assert.That(profile.ElementPaths.Keys, Does.Contain("/orders/order"));
            Assert.That(profile.ElementPaths.Keys, Does.Contain("/orders/order/customer"));
            Assert.That(profile.AttributePaths.Keys, Does.Contain("/orders/order/@id"));
            Assert.That(profile.ElementPaths["/orders/order"].OccurrenceCount, Is.EqualTo(2));
            Assert.That(profile.AttributePaths["/orders/order/@id"].OccurrenceCount, Is.EqualTo(2));
            Assert.That(profile.MaxDepth, Is.EqualTo(3));
        }

        [Test]
        public void ProfileDetectsRepeatedElementsAndCandidateRows()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order><number>100</number><items><item><sku>A</sku></item><item><sku>B</sku></item></items></order>" +
                "<order><number>101</number><items><item><sku>C</sku></item></items></order>" +
                "</orders>");

            XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlPath);

            Assert.That(profile.RepeatedElementPaths, Does.Contain("/orders/order"));
            Assert.That(profile.RepeatedElementPaths, Does.Contain("/orders/order/items/item"));
            Assert.That(profile.CandidateRowPaths, Does.Contain("/orders/order"));
            Assert.That(profile.CandidateRowPaths, Does.Contain("/orders/order/items/item"));
        }

        [Test]
        public void ProfileBuildsCandidateColumnsPerRowPath()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"100\"><number>100</number><customer>Alpha</customer><items><item sku=\"A\"><quantity>2</quantity></item></items></order>" +
                "<order id=\"101\"><number>101</number><customer>Beta</customer><items><item sku=\"B\"><quantity>4</quantity></item></items></order>" +
                "</orders>");

            XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlPath);

            string[] orderColumns = profile.GetCandidateColumns("/orders/order").ToArray();
            string[] itemColumns = profile.GetCandidateColumns("/orders/order/items/item").ToArray();

            Assert.That(orderColumns, Does.Contain("/orders/order/@id"));
            Assert.That(orderColumns, Does.Contain("/orders/order/customer"));
            Assert.That(orderColumns, Does.Contain("/orders/order/number"));
            Assert.That(orderColumns, Does.Not.Contain("/orders/order/items"));
            Assert.That(itemColumns, Does.Contain("/orders/order/items/item/@sku"));
            Assert.That(itemColumns, Does.Contain("/orders/order/items/item/quantity"));
        }

        [Test]
        public void ProfileInfersTypeHintsFromLeafTextAndAttributes()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<rows>" +
                "<row id=\"1\"><amount>10.25</amount><active>true</active><name>Alpha</name></row>" +
                "<row id=\"2\"><amount>11.50</amount><active>false</active><name>Beta</name></row>" +
                "</rows>");

            XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlPath);

            Assert.That(profile.AttributePaths["/rows/row/@id"].TypeHints.BestGuess, Is.EqualTo("integer"));
            Assert.That(profile.ElementPaths["/rows/row/amount"].TypeHints.BestGuess, Is.EqualTo("decimal"));
            Assert.That(profile.ElementPaths["/rows/row/active"].TypeHints.BestGuess, Is.EqualTo("boolean"));
            Assert.That(profile.ElementPaths["/rows/row/name"].TypeHints.BestGuess, Is.EqualTo("string"));
        }

        private static string WriteTempXml(string xml)
        {
            string xmlPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".xml");
            File.WriteAllText(xmlPath, xml, Encoding.UTF8);
            return xmlPath;
        }
    }
}
