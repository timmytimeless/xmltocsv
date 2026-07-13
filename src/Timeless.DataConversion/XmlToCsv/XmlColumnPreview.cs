namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlColumnPreview
{
    internal XmlColumnPreview(string path, string name, string typeHint)
    {
        Path = path;
        Name = name;
        TypeHint = typeHint;
    }

    public string Path { get; }
    public string Name { get; }
    public string TypeHint { get; }
}
