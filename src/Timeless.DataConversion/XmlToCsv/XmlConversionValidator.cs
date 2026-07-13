using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionValidator
{
    public XmlConversionValidationResult ValidateSourceFile(string xmlSourceFilePath, XmlConversionLimits limits)
    {
        var issues = new List<XmlConversionValidationIssue>();

        if (limits?.MaxFileSizeBytes is long maxFileSizeBytes)
        {
            long fileSize = new FileInfo(xmlSourceFilePath).Length;

            if (fileSize > maxFileSizeBytes)
            {
                issues.Add(XmlConversionValidationIssue.Create("max_file_size", "XML file size in bytes", fileSize, maxFileSizeBytes));
            }
        }

        return new XmlConversionValidationResult(issues);
    }

    public XmlConversionValidationResult ValidateStructuralProfile(XmlStructuralProfile profile, XmlConversionLimits limits)
    {
        var issues = new List<XmlConversionValidationIssue>();

        if (limits == null)
        {
            return new XmlConversionValidationResult(issues);
        }

        if (limits.MaxXmlDepth is int maxXmlDepth && profile.MaxDepth > maxXmlDepth)
        {
            issues.Add(XmlConversionValidationIssue.Create("max_xml_depth", "XML depth", profile.MaxDepth, maxXmlDepth));
        }

        if (limits.MaxUniquePaths is int maxUniquePaths)
        {
            int uniquePaths = profile.ElementPaths.Count + profile.AttributePaths.Count;

            if (uniquePaths > maxUniquePaths)
            {
                issues.Add(XmlConversionValidationIssue.Create("max_unique_paths", "Unique XML paths", uniquePaths, maxUniquePaths));
            }
        }

        return new XmlConversionValidationResult(issues);
    }

    public XmlConversionValidationResult ValidateTablePlan(XmlInferredTablePlan plan, XmlConversionLimits limits)
    {
        var issues = new List<XmlConversionValidationIssue>();

        if (limits == null)
        {
            return new XmlConversionValidationResult(issues);
        }

        if (limits.MaxGeneratedCsvFiles is int maxGeneratedCsvFiles && plan.Tables.Count > maxGeneratedCsvFiles)
        {
            issues.Add(XmlConversionValidationIssue.Create("max_generated_csv_files", "Generated CSV file count", plan.Tables.Count, maxGeneratedCsvFiles));
        }

        if (limits.MaxColumnsPerTable is int maxColumnsPerTable)
        {
            foreach (XmlInferredTable table in plan.Tables.Where(item => item.Columns.Count > maxColumnsPerTable))
            {
                issues.Add(XmlConversionValidationIssue.Create(
                    "max_columns_per_table",
                    "Column count for table '" + table.Name + "'",
                    table.Columns.Count,
                    maxColumnsPerTable));
            }
        }

        return new XmlConversionValidationResult(issues);
    }

    public XmlConversionValidationResult ValidateOutputDirectory(string outputDirectory, XmlConversionLimits limits)
    {
        var issues = new List<XmlConversionValidationIssue>();

        if (limits?.MaxOutputBytes is long maxOutputBytes)
        {
            long outputBytes = Directory.Exists(outputDirectory)
                ? Directory.GetFiles(outputDirectory, "*", SearchOption.AllDirectories).Sum(path => new FileInfo(path).Length)
                : 0;

            if (outputBytes > maxOutputBytes)
            {
                issues.Add(XmlConversionValidationIssue.Create("max_output_bytes", "Output size in bytes", outputBytes, maxOutputBytes));
            }
        }

        return new XmlConversionValidationResult(issues);
    }

    public XmlConversionValidationResult ValidateExecution(TimeSpan elapsed, XmlConversionLimits limits)
    {
        var issues = new List<XmlConversionValidationIssue>();

        if (limits == null)
        {
            return new XmlConversionValidationResult(issues);
        }

        if (limits.CancellationToken.IsCancellationRequested)
        {
            issues.Add(XmlConversionValidationIssue.CreateMessage("conversion_cancelled", "XML conversion was cancelled."));
        }

        if (limits.Timeout is TimeSpan timeout && elapsed > timeout)
        {
            issues.Add(XmlConversionValidationIssue.Create(
                "conversion_timeout",
                "Conversion elapsed milliseconds",
                (long)elapsed.TotalMilliseconds,
                (long)timeout.TotalMilliseconds));
        }

        return new XmlConversionValidationResult(issues);
    }
}
