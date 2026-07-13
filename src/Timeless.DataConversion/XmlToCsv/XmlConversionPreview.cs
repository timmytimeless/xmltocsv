using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionPreview
{
    private readonly List<XmlTablePreview> _tables;
    private readonly List<string> _warnings;

    internal XmlConversionPreview(XmlInferredTablePlan inferredPlan, IEnumerable<XmlTablePreview> tables, IEnumerable<string> warnings)
    {
        InferredPlan = inferredPlan;
        _tables = tables.ToList();
        _warnings = warnings.ToList();
    }

    public XmlInferredTablePlan InferredPlan { get; }
    public IReadOnlyList<XmlTablePreview> Tables => new ReadOnlyCollection<XmlTablePreview>(_tables);
    public IReadOnlyList<string> Warnings => new ReadOnlyCollection<string>(_warnings);
}
