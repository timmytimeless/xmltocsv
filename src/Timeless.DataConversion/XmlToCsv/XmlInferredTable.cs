using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlInferredTable
{
    private readonly List<XmlInferredColumn> _columns;
    private readonly List<XmlInferredTable> _childTables;
    private readonly List<string> _reasons;

    internal XmlInferredTable(
        string path,
        string name,
        long rowCount,
        int score,
        IEnumerable<XmlInferredColumn> columns,
        IEnumerable<string> reasons)
    {
        Path = path;
        Name = name;
        RowCount = rowCount;
        Score = score;
        _columns = columns.ToList();
        _childTables = new List<XmlInferredTable>();
        _reasons = reasons.ToList();
    }

    public string Path { get; }
    public string Name { get; }
    public long RowCount { get; }
    public int Score { get; }
    public IReadOnlyList<XmlInferredColumn> Columns => new ReadOnlyCollection<XmlInferredColumn>(_columns);
    public IReadOnlyList<XmlInferredTable> ChildTables => new ReadOnlyCollection<XmlInferredTable>(_childTables);
    public IReadOnlyList<string> Reasons => new ReadOnlyCollection<string>(_reasons);

    internal void AddChildTable(XmlInferredTable table)
    {
        _childTables.Add(table);
    }
}
