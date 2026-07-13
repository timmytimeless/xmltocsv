using System;
using System.Globalization;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlTypeHints
{
    private bool _hasValue;

    internal void Observe(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        _hasValue = true;
        IsInteger &= long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        IsDecimal &= decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
        IsBoolean &= bool.TryParse(value, out _);
        IsDateTime &= DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    public bool HasValues => _hasValue;
    public bool IsInteger { get; private set; } = true;
    public bool IsDecimal { get; private set; } = true;
    public bool IsBoolean { get; private set; } = true;
    public bool IsDateTime { get; private set; } = true;

    public string BestGuess
    {
        get
        {
            if (!_hasValue)
            {
                return "empty";
            }

            if (IsInteger)
            {
                return "integer";
            }

            if (IsDecimal)
            {
                return "decimal";
            }

            if (IsBoolean)
            {
                return "boolean";
            }

            if (IsDateTime)
            {
                return "datetime";
            }

            return "string";
        }
    }
}
