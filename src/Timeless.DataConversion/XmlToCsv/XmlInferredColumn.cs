namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlInferredColumn
{
    public XmlInferredColumn(string path, string name, string typeHint)
    {
        Path = path;
        Name = name;
        TypeHint = typeHint;
    }

    public string Path { get; }
    public string Name { get; }
    public string TypeHint { get; }
}
