using System;
using System.Collections.Generic;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionSample
{
    public XmlConversionSample(IEnumerable<XmlTableSample> tables, int maximumRowsPerTable)
    {
        Tables = (tables ?? Array.Empty<XmlTableSample>()).ToArray();
        MaximumRowsPerTable = maximumRowsPerTable;
    }

    public IReadOnlyList<XmlTableSample> Tables { get; }
    public int MaximumRowsPerTable { get; }
}

public sealed class XmlTableSample
{
    public XmlTableSample(
        string path,
        string name,
        long totalRowCount,
        IEnumerable<string> columns,
        IEnumerable<XmlSampleRow> rows)
    {
        Path = path;
        Name = name;
        TotalRowCount = totalRowCount;
        Columns = (columns ?? Array.Empty<string>()).ToArray();
        Rows = (rows ?? Array.Empty<XmlSampleRow>()).ToArray();
    }

    public string Path { get; }
    public string Name { get; }
    public long TotalRowCount { get; }
    public IReadOnlyList<string> Columns { get; }
    public IReadOnlyList<XmlSampleRow> Rows { get; }
    public bool IsTruncated => TotalRowCount > Rows.Count;
}

public sealed class XmlSampleRow
{
    public XmlSampleRow(IEnumerable<string> values)
    {
        Values = (values ?? Array.Empty<string>()).ToArray();
    }

    public IReadOnlyList<string> Values { get; }
}
