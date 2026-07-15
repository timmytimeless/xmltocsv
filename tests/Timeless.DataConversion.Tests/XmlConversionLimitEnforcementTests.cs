using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Timeless.DataConversion.XmlToCsv;
using NUnit.Framework;

namespace Timeless.DataConversion.Tests
{
    public class XmlConversionLimitEnforcementTests
    {
        [Test]
        public void CreateConversionPreviewEnforcesMaximumXmlDepth()
        {
            var xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?><root><level1><level2><row><name>A</name></row></level2></level1></root>");
            var limits = new XmlConversionLimits { MaxXmlDepth = 3 };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.CreateConversionPreview(xmlPath, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("max_xml_depth"));
        }

        [Test]
        public void CreateConversionPreviewEnforcesMaximumFileSizeBeforeProfiling()
        {
            var xmlPath = WriteTempXml("<?xml version=\"1.0\"?><root><row><name>A</name></row></root>");
            var limits = new XmlConversionLimits { MaxFileSizeBytes = 10 };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.CreateConversionPreview(xmlPath, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("max_file_size"));
        }

        [Test]
        public void ExportConfirmedConversionEnforcesConfirmedPlanLimits()
        {
            var xmlPath = WriteOrdersXml();
            var preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            var confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            var outputDirectory = CreateOutputDirectory();
            var limits = new XmlConversionLimits { MaxGeneratedCsvFiles = 1 };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.ExportConfirmedConversion(xmlPath, outputDirectory, Encoding.UTF8, preview, confirmation, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("max_generated_csv_files"));
        }

        [Test]
        public void ExportConfirmedConversionEnforcesOutputSizeLimitAfterExport()
        {
            var xmlPath = WriteOrdersXml();
            var preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            var confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            var outputDirectory = CreateOutputDirectory();
            var limits = new XmlConversionLimits { MaxOutputBytes = 1 };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.ExportConfirmedConversion(xmlPath, outputDirectory, Encoding.UTF8, preview, confirmation, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("max_output_bytes"));
        }

        [Test]
        public void ExportConfirmedConversionEnforcesCancellation()
        {
            var xmlPath = WriteOrdersXml();
            var preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            var confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            var outputDirectory = CreateOutputDirectory();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var limits = new XmlConversionLimits { CancellationToken = cancellationTokenSource.Token };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.ExportConfirmedConversion(xmlPath, outputDirectory, Encoding.UTF8, preview, confirmation, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("conversion_cancelled"));
        }

        [Test]
        public void CreateConversionPreviewStopsWhenUniquePathLimitIsExceeded()
        {
            var xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?><root><row><a>1</a><b>2</b><c>3</c></row></root>");
            var limits = new XmlConversionLimits { MaxUniquePaths = 3 };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.CreateConversionPreview(xmlPath, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("max_unique_paths"));
        }

        [Test]
        public void ExportConfirmedConversionEnforcesRowSubtreeLimitDuringExport()
        {
            var xmlPath = WriteOrdersXml();
            var preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            var confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            var outputDirectory = CreateOutputDirectory();
            var limits = new XmlConversionLimits { MaxRowSubtreeBytes = 1 };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.ExportConfirmedConversion(xmlPath, outputDirectory, Encoding.UTF8, preview, confirmation, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("max_row_subtree_bytes"));
        }

        [Test]
        public void ExportConfirmedConversionToZipEnforcesZipSizeLimit()
        {
            var xmlPath = WriteOrdersXml();
            var preview = XmlToCsvConverter.CreateConversionPreview(xmlPath);
            var confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);
            var outputZipPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".zip");
            var limits = new XmlConversionLimits { MaxOutputZipBytes = 1 };

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                XmlToCsvConverter.ExportConfirmedConversionToZip(xmlPath, outputZipPath, Encoding.UTF8, preview, confirmation, limits));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("max_output_zip_bytes"));
        }

        [Test]
        public void PublicConversionServiceRejectsAmbiguousLowConfidencePlans()
        {
            var xmlPath = WriteTempXml(
                "<?xml version=\"1.0\"?>" +
                "<orders>" +
                "<order><number>100</number><items><item><quantity>2</quantity></item></items></order>" +
                "<order><number>101</number><items><item><quantity>3</quantity></item></items></order>" +
                "</orders>");
            var service = new XmlPublicConversionService(new XmlConversionLimits());

            var exception = Assert.Throws<XmlConversionValidationException>(() =>
                service.CreatePreview(xmlPath));

            Assert.That(exception.Result.Issues.Single().Code, Is.EqualTo("low_confidence_table"));
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
            var xmlPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName() + ".xml");
            File.WriteAllText(xmlPath, xml, Encoding.UTF8);
            return xmlPath;
        }

        private static string CreateOutputDirectory()
        {
            var outputDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, Path.GetRandomFileName());
            Directory.CreateDirectory(outputDirectory);
            return outputDirectory;
        }
    }
}
