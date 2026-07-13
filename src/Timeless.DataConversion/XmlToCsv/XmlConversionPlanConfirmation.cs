using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionPlanConfirmation
{
    private readonly List<XmlTablePlanConfirmation> _tables;

    public XmlConversionPlanConfirmation(IEnumerable<XmlTablePlanConfirmation> tables)
    {
        _tables = tables.ToList();
    }

    public IReadOnlyList<XmlTablePlanConfirmation> Tables => new ReadOnlyCollection<XmlTablePlanConfirmation>(_tables);

    public static XmlConversionPlanConfirmation IncludeAll(XmlConversionPreview preview)
    {
        return new XmlConversionPlanConfirmation(preview.Tables.Select(CreateTableConfirmation));
    }

    private static XmlTablePlanConfirmation CreateTableConfirmation(XmlTablePreview table)
    {
        return new XmlTablePlanConfirmation(
            table.Path,
            table.Columns.Select(column => new XmlColumnPlanConfirmation(column.Path)));
    }
}
