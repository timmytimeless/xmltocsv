using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionValidationResult
{
    private readonly List<XmlConversionValidationIssue> _issues;

    internal XmlConversionValidationResult(IEnumerable<XmlConversionValidationIssue> issues)
    {
        _issues = issues.ToList();
    }

    public bool IsValid => _issues.Count == 0;
    public IReadOnlyList<XmlConversionValidationIssue> Issues => new ReadOnlyCollection<XmlConversionValidationIssue>(_issues);
}
