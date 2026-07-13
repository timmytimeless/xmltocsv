using System;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionValidationException : Exception
{
    public XmlConversionValidationException(XmlConversionValidationResult result)
        : base(result?.Issues.Count > 0 ? result.Issues[0].Message : "XML conversion validation failed.")
    {
        Result = result ?? new XmlConversionValidationResult(Array.Empty<XmlConversionValidationIssue>());
    }

    public XmlConversionValidationResult Result { get; }
}
