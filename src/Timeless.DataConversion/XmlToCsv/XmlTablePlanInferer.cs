using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlTablePlanInferer
{
    public XmlInferredTablePlan InferTables(XmlStructuralProfile profile)
    {
        var tables = new List<XmlInferredTable>();

        foreach (string rowPath in profile.CandidateRowPaths.OrderBy(item => item))
        {
            if (!profile.ElementPaths.TryGetValue(rowPath, out XmlPathProfile pathProfile))
            {
                continue;
            }

            IReadOnlyCollection<string> columnPaths = profile.GetCandidateColumns(rowPath);
            int score = CalculateScore(pathProfile, columnPaths);

            if (score <= 0)
            {
                continue;
            }

            tables.Add(new XmlInferredTable(
                rowPath,
                CreateTableName(rowPath),
                pathProfile.OccurrenceCount,
                score,
                CreateColumns(profile, columnPaths),
                CreateReasons(pathProfile, columnPaths, score)));
        }

        LinkChildTables(tables);
        return new XmlInferredTablePlan(tables.OrderByDescending(item => item.Score).ThenBy(item => item.Path));
    }

    private static int CalculateScore(XmlPathProfile pathProfile, IReadOnlyCollection<string> columnPaths)
    {
        int score = 0;

        if (pathProfile.IsRepeatedElement)
        {
            score += 50;
        }

        if (pathProfile.OccurrenceCount > 1)
        {
            score += 30;
        }

        if (columnPaths.Count >= 2)
        {
            score += 20;
        }
        else if (columnPaths.Count == 1)
        {
            score += 10;
        }

        if (pathProfile.OccurrenceCount >= 10)
        {
            score += 10;
        }

        if (HasStableRepeatedStructure(pathProfile))
        {
            score += 20;
        }

        return score;
    }

    private static bool HasStableRepeatedStructure(XmlPathProfile pathProfile)
    {
        return pathProfile.OccurrenceCount > 1 &&
               pathProfile.StructureSignatureCount > 0 &&
               pathProfile.DominantStructureSignatureCount * 100 / pathProfile.OccurrenceCount >= 80;
    }

    private static IEnumerable<XmlInferredColumn> CreateColumns(XmlStructuralProfile profile, IEnumerable<string> columnPaths)
    {
        foreach (string columnPath in columnPaths.OrderBy(item => item))
        {
            XmlPathProfile pathProfile = GetColumnProfile(profile, columnPath);
            string typeHint = pathProfile == null ? "string" : pathProfile.TypeHints.BestGuess;
            yield return new XmlInferredColumn(columnPath, CreateColumnName(columnPath), typeHint);
        }
    }

    private static XmlPathProfile GetColumnProfile(XmlStructuralProfile profile, string columnPath)
    {
        if (profile.ElementPaths.TryGetValue(columnPath, out XmlPathProfile elementPathProfile))
        {
            return elementPathProfile;
        }

        if (profile.AttributePaths.TryGetValue(columnPath, out XmlPathProfile attributePathProfile))
        {
            return attributePathProfile;
        }

        return null;
    }

    private static IEnumerable<string> CreateReasons(XmlPathProfile pathProfile, IReadOnlyCollection<string> columnPaths, int score)
    {
        if (pathProfile.IsRepeatedElement)
        {
            yield return "repeated sibling element";
        }

        if (pathProfile.OccurrenceCount > 1)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "observed {0} rows", pathProfile.OccurrenceCount);
        }

        if (columnPaths.Count > 0)
        {
            yield return string.Format(CultureInfo.InvariantCulture, "{0} candidate columns", columnPaths.Count);
        }

        if (HasStableRepeatedStructure(pathProfile))
        {
            yield return string.Format(
                CultureInfo.InvariantCulture,
                "stable structure across {0} of {1} rows",
                pathProfile.DominantStructureSignatureCount,
                pathProfile.OccurrenceCount);
        }

        yield return string.Format(CultureInfo.InvariantCulture, "score {0}", score);
    }

    private static void LinkChildTables(List<XmlInferredTable> tables)
    {
        foreach (XmlInferredTable table in tables)
        {
            XmlInferredTable parent = tables
                .Where(item => item != table && IsDescendantPath(table.Path, item.Path))
                .OrderByDescending(item => item.Path.Length)
                .FirstOrDefault();

            parent?.AddChildTable(table);
        }
    }

    private static bool IsDescendantPath(string childPath, string parentPath)
    {
        return childPath.Length > parentPath.Length &&
               childPath.StartsWith(parentPath + "/", System.StringComparison.Ordinal);
    }

    private static string CreateTableName(string path)
    {
        string[] segments = path.Split('/').Where(item => item.Length > 0).ToArray();

        if (segments.Length == 0)
        {
            return "table";
        }

        if (segments.Length == 1)
        {
            return NormalizeName(segments[0]);
        }

        return NormalizeName(segments[segments.Length - 2] + "_" + segments[segments.Length - 1]);
    }

    private static string CreateColumnName(string path)
    {
        int index = path.LastIndexOf('/');
        string name = index >= 0 ? path.Substring(index + 1) : path;

        if (name.StartsWith("@", System.StringComparison.Ordinal))
        {
            name = name.Substring(1);
        }

        return NormalizeName(name);
    }

    private static string NormalizeName(string name)
    {
        return name.Replace('-', '_').Replace('.', '_');
    }
}
