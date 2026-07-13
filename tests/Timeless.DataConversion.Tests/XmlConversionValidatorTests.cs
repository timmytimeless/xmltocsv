using System.IO;
using System.Linq;
using System.Text;
using Timeless.DataConversion.XmlToCsv;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    public class XmlConversionValidatorTests
    {
        [Test]
        public void ValidateSourceFileReportsFileSizeLimit()
        {
            string xmlPath = WriteTempXml("<?xml version=\"1.0\"?><root><row><name>A</name></row></root>");
            var limits = new XmlConversionLimits { MaxFileSizeBytes = 10 };

            XmlConversionValidationResult result = XmlToCsvConverter.ValidateSourceFile(xmlPath, limits);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues.Single().Code, Is.EqualTo("max_file_size"));
        }

        [Test]
        public void ValidateStructuralProfileReportsDepthAndUniquePathLimits()
        {
            XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(WriteTempXml(
                "<?xml version=\"1.0\"?><root><level1><level2><row id=\"1\"><name>A</name></row></level2></level1></root>"));
            var limits = new XmlConversionLimits
            {
                MaxXmlDepth = 3,
                MaxUniquePaths = 2
            };

            XmlConversionValidationResult result = XmlToCsvConverter.ValidateStructuralProfile(profile, limits);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues.Select(item => item.Code), Does.Contain("max_xml_depth"));
            Assert.That(result.Issues.Select(item => item.Code), Does.Contain("max_unique_paths"));
        }

        [Test]
        public void ValidateTablePlanReportsTableAndColumnLimits()
        {
            XmlInferredTablePlan plan = XmlToCsvConverter.InferTablePlan(WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order id=\"1\"><number>1</number><customer>A</customer><items><item><sku>A</sku></item></items></order>" +
                "<order id=\"2\"><number>2</number><customer>B</customer><items><item><sku>B</sku></item></items></order>" +
                "</orders>"));
            var limits = new XmlConversionLimits
            {
                MaxGeneratedCsvFiles = 1,
                MaxColumnsPerTable = 2
            };

            XmlConversionValidationResult result = XmlToCsvConverter.ValidateTablePlan(plan, limits);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues.Select(item => item.Code), Does.Contain("max_generated_csv_files"));
            Assert.That(result.Issues.Select(item => item.Code), Does.Contain("max_columns_per_table"));
        }

        [Test]
        public void ValidateOutputDirectoryReportsOutputSizeLimit()
        {
            string outputDirectory = CreateOutputDirectory();
            File.WriteAllText(Path.Combine(outputDirectory, "data.csv"), "0123456789", Encoding.UTF8);
            var limits = new XmlConversionLimits { MaxOutputBytes = 3 };

            XmlConversionValidationResult result = XmlToCsvConverter.ValidateOutputDirectory(outputDirectory, limits);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Issues.Single().Code, Is.EqualTo("max_output_bytes"));
        }

        [Test]
        public void ValidatorsReturnValidWhenLimitsAreNotExceeded()
        {
            string xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?><root><row><name>A</name></row><row><name>B</name></row></root>");
            XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlPath);
            XmlInferredTablePlan plan = new XmlTablePlanInferer().InferTables(profile);
            var limits = new XmlConversionLimits
            {
                MaxFileSizeBytes = 1024,
                MaxXmlDepth = 4,
                MaxUniquePaths = 10,
                MaxGeneratedCsvFiles = 2,
                MaxColumnsPerTable = 2,
                MaxOutputBytes = 1024
            };

            Assert.That(XmlToCsvConverter.ValidateSourceFile(xmlPath, limits).IsValid, Is.True);
            Assert.That(XmlToCsvConverter.ValidateStructuralProfile(profile, limits).IsValid, Is.True);
            Assert.That(XmlToCsvConverter.ValidateTablePlan(plan, limits).IsValid, Is.True);
            Assert.That(XmlToCsvConverter.ValidateOutputDirectory(CreateOutputDirectory(), limits).IsValid, Is.True);
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
