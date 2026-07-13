using System.Diagnostics;
using System.IO;
using System.Text;
using Timeless.DataConversion.XmlToCsv;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    [Explicit("Performance benchmark for local comparison; timings are environment-dependent.")]
    public class XmlToCsvPerformanceTests
    {
        private const int RowCount = 10000;

        [Test]
        public void BenchmarkLargeFlatXmlConversion()
        {
            string xmlPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "large-flat-conversion-benchmark.xml");
            string csvPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "large-flat-conversion-benchmark.csv");
            WriteLargeFlatXml(xmlPath, RowCount);

            Stopwatch loadTimer = Stopwatch.StartNew();
            using var converter = new XmlToCsvUsingDataSet(xmlPath);
            loadTimer.Stop();

            Stopwatch exportTimer = Stopwatch.StartNew();
            converter.ExportToCsv("row", csvPath, Encoding.UTF8);
            exportTimer.Stop();

            long csvSize = new FileInfo(csvPath).Length;

            TestContext.Out.WriteLine("Rows: {0}", RowCount);
            TestContext.Out.WriteLine("XML bytes: {0}", new FileInfo(xmlPath).Length);
            TestContext.Out.WriteLine("CSV bytes: {0}", csvSize);
            TestContext.Out.WriteLine("Load ms: {0}", loadTimer.ElapsedMilliseconds);
            TestContext.Out.WriteLine("Export ms: {0}", exportTimer.ElapsedMilliseconds);

            Assert.That(converter.TableNameCollection, Does.Contain("row"));
            Assert.That(csvSize, Is.GreaterThan(0));
        }

        private static void WriteLargeFlatXml(string path, int rowCount)
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            writer.WriteLine("<root>");

            for (int i = 0; i < rowCount; i++)
            {
                writer.WriteLine("  <row>");
                writer.WriteLine("    <Id>{0}</Id>", i);
                writer.WriteLine("    <Name>Name {0}</Name>", i);
                writer.WriteLine("    <Description>Value with comma, quote &quot;{0}&quot;, and text</Description>", i);
                writer.WriteLine("    <Amount>{0}</Amount>", i * 1.25m);
                writer.WriteLine("  </row>");
            }

            writer.WriteLine("</root>");
        }
    }
}
