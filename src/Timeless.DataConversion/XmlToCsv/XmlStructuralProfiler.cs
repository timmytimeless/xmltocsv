using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlStructuralProfiler
{
    public XmlStructuralProfile Profile(string xmlSourceFilePath)
    {
        using XmlReader reader = XmlReader.Create(xmlSourceFilePath, CreateReaderSettings());
        return Profile(reader);
    }

    public XmlStructuralProfile Profile(Stream xmlSource)
    {
        using XmlReader reader = XmlReader.Create(xmlSource, CreateReaderSettings());
        return Profile(reader);
    }

    private static XmlStructuralProfile Profile(XmlReader reader)
    {
        var elementPaths = new Dictionary<string, XmlPathProfile>();
        var attributePaths = new Dictionary<string, XmlPathProfile>();
        var repeatedElementPaths = new HashSet<string>();
        var candidateColumnsByRowPath = new Dictionary<string, HashSet<string>>();
        var stack = new Stack<ElementFrame>();
        int maxDepth = 0;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    ProcessElement(reader, stack, elementPaths, attributePaths, repeatedElementPaths, candidateColumnsByRowPath, ref maxDepth);
                    break;
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.SignificantWhitespace:
                    if (stack.Count > 0)
                    {
                        stack.Peek().Text.Append(reader.Value);
                    }
                    break;
                case XmlNodeType.EndElement:
                    CompleteElement(stack, candidateColumnsByRowPath, elementPaths);
                    break;
            }
        }

        var candidateRowPaths = InferCandidateRowPaths(elementPaths, candidateColumnsByRowPath);
        return new XmlStructuralProfile(elementPaths, attributePaths, repeatedElementPaths, candidateColumnsByRowPath, candidateRowPaths, maxDepth);
    }

    private static void ProcessElement(
        XmlReader reader,
        Stack<ElementFrame> stack,
        Dictionary<string, XmlPathProfile> elementPaths,
        Dictionary<string, XmlPathProfile> attributePaths,
        HashSet<string> repeatedElementPaths,
        Dictionary<string, HashSet<string>> candidateColumnsByRowPath,
        ref int maxDepth)
    {
        string parentPath = stack.Count == 0 ? null : stack.Peek().Path;
        string path = BuildPath(parentPath, reader.LocalName);
        int depth = stack.Count + 1;
        maxDepth = System.Math.Max(maxDepth, depth);

        XmlPathProfile profile = GetPathProfile(elementPaths, path);
        profile.OccurrenceCount++;
        profile.MaxDepth = System.Math.Max(profile.MaxDepth, depth);

        if (stack.Count > 0)
        {
            ElementFrame parent = stack.Peek();
            parent.HasChildElements = true;
            parent.ChildOccurrenceCounts.TryGetValue(path, out int childCount);
            childCount++;
            parent.ChildOccurrenceCounts[path] = childCount;

            if (childCount > 1)
            {
                repeatedElementPaths.Add(path);
                profile.IsRepeatedElement = true;
            }
        }

        var frame = new ElementFrame(path);
        AddAttributes(reader, frame, attributePaths, candidateColumnsByRowPath, depth);

        if (reader.IsEmptyElement)
        {
            AddLeafColumnToParent(stack, path, candidateColumnsByRowPath);
            return;
        }

        stack.Push(frame);
    }

    private static void AddAttributes(
        XmlReader reader,
        ElementFrame frame,
        Dictionary<string, XmlPathProfile> attributePaths,
        Dictionary<string, HashSet<string>> candidateColumnsByRowPath,
        int elementDepth)
    {
        if (!reader.HasAttributes)
        {
            return;
        }

        while (reader.MoveToNextAttribute())
        {
            string attributePath = frame.Path + "/@" + reader.LocalName;
            XmlPathProfile attributeProfile = GetPathProfile(attributePaths, attributePath);
            attributeProfile.OccurrenceCount++;
            attributeProfile.MaxDepth = System.Math.Max(attributeProfile.MaxDepth, elementDepth);
            attributeProfile.TypeHints.Observe(reader.Value);
            AddCandidateColumn(candidateColumnsByRowPath, frame.Path, attributePath);
        }

        reader.MoveToElement();
    }

    private static void CompleteElement(
        Stack<ElementFrame> stack,
        Dictionary<string, HashSet<string>> candidateColumnsByRowPath,
        Dictionary<string, XmlPathProfile> elementPaths)
    {
        if (stack.Count == 0)
        {
            return;
        }

        ElementFrame frame = stack.Pop();
        XmlPathProfile profile = elementPaths[frame.Path];
        profile.TypeHints.Observe(frame.Text.ToString());

        if (!frame.HasChildElements)
        {
            AddLeafColumnToParent(stack, frame.Path, candidateColumnsByRowPath);
        }
    }

    private static void AddLeafColumnToParent(
        Stack<ElementFrame> stack,
        string leafPath,
        Dictionary<string, HashSet<string>> candidateColumnsByRowPath)
    {
        if (stack.Count == 0)
        {
            return;
        }

        AddCandidateColumn(candidateColumnsByRowPath, stack.Peek().Path, leafPath);
    }

    private static HashSet<string> InferCandidateRowPaths(
        Dictionary<string, XmlPathProfile> elementPaths,
        Dictionary<string, HashSet<string>> candidateColumnsByRowPath)
    {
        var candidateRowPaths = new HashSet<string>();

        foreach (KeyValuePair<string, HashSet<string>> item in candidateColumnsByRowPath)
        {
            if (!elementPaths.TryGetValue(item.Key, out XmlPathProfile profile))
            {
                continue;
            }

            if (profile.OccurrenceCount > 1 || profile.IsRepeatedElement)
            {
                candidateRowPaths.Add(item.Key);
            }
        }

        return candidateRowPaths;
    }

    private static void AddCandidateColumn(Dictionary<string, HashSet<string>> candidateColumnsByRowPath, string rowPath, string columnPath)
    {
        if (!candidateColumnsByRowPath.TryGetValue(rowPath, out HashSet<string> columns))
        {
            columns = new HashSet<string>();
            candidateColumnsByRowPath.Add(rowPath, columns);
        }

        columns.Add(columnPath);
    }

    private static XmlPathProfile GetPathProfile(Dictionary<string, XmlPathProfile> profiles, string path)
    {
        if (!profiles.TryGetValue(path, out XmlPathProfile profile))
        {
            profile = new XmlPathProfile(path);
            profiles.Add(path, profile);
        }

        return profile;
    }

    private static string BuildPath(string parentPath, string localName)
    {
        return string.IsNullOrEmpty(parentPath) ? "/" + localName : parentPath + "/" + localName;
    }

    private static XmlReaderSettings CreateReaderSettings()
    {
        return new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };
    }

    private sealed class ElementFrame
    {
        public ElementFrame(string path)
        {
            Path = path;
            Text = new System.Text.StringBuilder();
            ChildOccurrenceCounts = new Dictionary<string, int>();
        }

        public string Path { get; }
        public bool HasChildElements { get; set; }
        public System.Text.StringBuilder Text { get; }
        public Dictionary<string, int> ChildOccurrenceCounts { get; }
    }
}
