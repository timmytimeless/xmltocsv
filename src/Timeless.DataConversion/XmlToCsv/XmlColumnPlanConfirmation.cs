namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlColumnPlanConfirmation
{
    public XmlColumnPlanConfirmation(string path)
    {
        Path = path;
        Include = true;
    }

    public string Path { get; }
    public bool Include { get; set; }
    public string Name { get; set; }
}
