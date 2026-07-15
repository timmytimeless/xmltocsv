namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlPathProfile
{
    internal XmlPathProfile(string path)
    {
        Path = path;
        TypeHints = new XmlTypeHints();
        StructureSignatures = new System.Collections.Generic.Dictionary<string, long>();
    }

    public string Path { get; }
    public long OccurrenceCount { get; internal set; }
    public int MaxDepth { get; internal set; }
    public bool IsRepeatedElement { get; internal set; }
    public XmlTypeHints TypeHints { get; }
    public int StructureSignatureCount => StructureSignatures.Count;
    public long DominantStructureSignatureCount => StructureSignatures.Count == 0 ? 0 : System.Linq.Enumerable.Max(StructureSignatures.Values);
    internal System.Collections.Generic.Dictionary<string, long> StructureSignatures { get; }
}
