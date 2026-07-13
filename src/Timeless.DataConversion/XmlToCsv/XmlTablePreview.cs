using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlTablePreview
{
    private readonly List<XmlColumnPreview> _columns;
    private readonly List<string> _childTablePaths;
    private readonly List<string> _reasons;

    internal XmlTablePreview(
        string path,
        string name,
        long rowCount,
        int score,
        IEnumerable<XmlColumnPreview> columns,
        IEnumerable<string> childTablePaths,
        IEnumerable<string> reasons)
    {
        Path = path;
        Name = name;
        RowCount = rowCount;
        Score = score;
        _columns = columns.ToList();
        _childTablePaths = childTablePaths.ToList();
        _reasons = reasons.ToList();
    }

    public string Path { get; }
    public string Name { get; }
    public long RowCount { get; }
    public int Score { get; }
    public IReadOnlyList<XmlColumnPreview> Columns => new ReadOnlyCollection<XmlColumnPreview>(_columns);
    public IReadOnlyList<string> ChildTablePaths => new ReadOnlyCollection<string>(_childTablePaths);
    public IReadOnlyList<string> Reasons => new ReadOnlyCollection<string>(_reasons);
}
