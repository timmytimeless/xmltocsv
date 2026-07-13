namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlPathProfile
{
    internal XmlPathProfile(string path)
    {
        Path = path;
        TypeHints = new XmlTypeHints();
    }

    public string Path { get; }
    public long OccurrenceCount { get; internal set; }
    public int MaxDepth { get; internal set; }
    public bool IsRepeatedElement { get; internal set; }
    public XmlTypeHints TypeHints { get; }
}
