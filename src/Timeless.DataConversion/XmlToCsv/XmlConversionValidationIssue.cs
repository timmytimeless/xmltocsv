using System.Globalization;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlConversionValidationIssue
{
    internal XmlConversionValidationIssue(string code, string message, long actualValue, long limit)
    {
        Code = code;
        Message = message;
        ActualValue = actualValue;
        Limit = limit;
    }

    public string Code { get; }
    public string Message { get; }
    public long ActualValue { get; }
    public long Limit { get; }

    internal static XmlConversionValidationIssue Create(string code, string subject, long actualValue, long limit)
    {
        string message = string.Format(
            CultureInfo.InvariantCulture,
            "{0} is {1}, which exceeds the configured limit of {2}.",
            subject,
            actualValue,
            limit);

        return new XmlConversionValidationIssue(code, message, actualValue, limit);
    }
}
