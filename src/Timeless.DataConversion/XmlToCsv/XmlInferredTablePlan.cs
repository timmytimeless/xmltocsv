using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlInferredTablePlan
{
    private readonly List<XmlInferredTable> _tables;

    internal XmlInferredTablePlan(IEnumerable<XmlInferredTable> tables)
    {
        _tables = tables.ToList();
    }

    public IReadOnlyList<XmlInferredTable> Tables => new ReadOnlyCollection<XmlInferredTable>(_tables);
}
