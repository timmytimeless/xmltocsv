using System;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionLimits
{
    public long? MaxFileSizeBytes { get; set; }
    public int? MaxXmlDepth { get; set; }
    public int? MaxUniquePaths { get; set; }
    public int? MaxColumnsPerTable { get; set; }
    public int? MaxGeneratedCsvFiles { get; set; }
    public long? MaxOutputBytes { get; set; }
    public TimeSpan? Timeout { get; set; }
}
