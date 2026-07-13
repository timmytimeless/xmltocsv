using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlTablePlanConfirmation
{
    private readonly List<XmlColumnPlanConfirmation> _columns;

    public XmlTablePlanConfirmation(string path, IEnumerable<XmlColumnPlanConfirmation> columns)
    {
        Path = path;
        Include = true;
        _columns = columns.ToList();
    }

    public string Path { get; }
    public bool Include { get; set; }
    public string Name { get; set; }
    public IReadOnlyList<XmlColumnPlanConfirmation> Columns => new ReadOnlyCollection<XmlColumnPlanConfirmation>(_columns);
}
