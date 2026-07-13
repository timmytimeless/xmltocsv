using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlStructuralProfile
{
    private readonly Dictionary<string, XmlPathProfile> _elementPaths;
    private readonly Dictionary<string, XmlPathProfile> _attributePaths;
    private readonly HashSet<string> _repeatedElementPaths;
    private readonly Dictionary<string, HashSet<string>> _candidateColumnsByRowPath;
    private readonly HashSet<string> _candidateRowPaths;

    internal XmlStructuralProfile(
        Dictionary<string, XmlPathProfile> elementPaths,
        Dictionary<string, XmlPathProfile> attributePaths,
        HashSet<string> repeatedElementPaths,
        Dictionary<string, HashSet<string>> candidateColumnsByRowPath,
        HashSet<string> candidateRowPaths,
        int maxDepth)
    {
        _elementPaths = elementPaths;
        _attributePaths = attributePaths;
        _repeatedElementPaths = repeatedElementPaths;
        _candidateColumnsByRowPath = candidateColumnsByRowPath;
        _candidateRowPaths = candidateRowPaths;
        MaxDepth = maxDepth;
    }

    public IReadOnlyDictionary<string, XmlPathProfile> ElementPaths => new ReadOnlyDictionary<string, XmlPathProfile>(_elementPaths);
    public IReadOnlyDictionary<string, XmlPathProfile> AttributePaths => new ReadOnlyDictionary<string, XmlPathProfile>(_attributePaths);
    public IReadOnlyCollection<string> RepeatedElementPaths => _repeatedElementPaths.ToList().AsReadOnly();
    public IReadOnlyCollection<string> CandidateRowPaths => _candidateRowPaths.ToList().AsReadOnly();
    public int MaxDepth { get; }

    public IReadOnlyCollection<string> GetCandidateColumns(string rowPath)
    {
        if (!_candidateColumnsByRowPath.TryGetValue(rowPath, out HashSet<string> columns))
        {
            return new List<string>().AsReadOnly();
        }

        return columns.OrderBy(item => item).ToList().AsReadOnly();
    }
}
