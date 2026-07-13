using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionPreviewBuilder
{
    public XmlConversionPreview Build(XmlInferredTablePlan plan)
    {
        List<XmlTablePreview> tables = plan.Tables.Select(CreateTablePreview).ToList();
        return new XmlConversionPreview(plan, tables, CreateWarnings(tables));
    }

    private static XmlTablePreview CreateTablePreview(XmlInferredTable table)
    {
        return new XmlTablePreview(
            table.Path,
            table.Name,
            table.RowCount,
            table.Score,
            table.Columns.Select(CreateColumnPreview),
            table.ChildTables.Select(item => item.Path),
            table.Reasons);
    }

    private static XmlColumnPreview CreateColumnPreview(XmlInferredColumn column)
    {
        return new XmlColumnPreview(column.Path, column.Name, column.TypeHint);
    }

    private static IEnumerable<string> CreateWarnings(IReadOnlyList<XmlTablePreview> tables)
    {
        if (tables.Count == 0)
        {
            yield return "No candidate tables were inferred from the XML structure.";
            yield break;
        }

        foreach (XmlTablePreview table in tables.Where(item => item.Columns.Count == 0))
        {
            yield return string.Format(CultureInfo.InvariantCulture, "Table '{0}' has no inferred columns.", table.Name);
        }

        foreach (XmlTablePreview table in tables.Where(item => item.Score < 80))
        {
            yield return string.Format(CultureInfo.InvariantCulture, "Table '{0}' has a low inference score and should be reviewed.", table.Name);
        }

        foreach (IGrouping<string, XmlTablePreview> duplicateNameGroup in tables.GroupBy(item => item.Name).Where(item => item.Count() > 1))
        {
            yield return string.Format(CultureInfo.InvariantCulture, "Multiple inferred tables use the name '{0}'. Rename one before export.", duplicateNameGroup.Key);
        }
    }
}
