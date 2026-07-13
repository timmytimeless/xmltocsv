using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Timeless.DataConversion.XmlToCsvStrategy
{
    public abstract class XmlToCsvStrategyBase
    {
        protected XmlToCsvStrategyBase()
        {
            HeaderColumnNameCollection = new Dictionary<int, string>(64);
            TableNameCollection = new Collection<string>();
        }

        public Dictionary<int, string> HeaderColumnNameCollection { get; private set; }
        protected int ColumnCount { get; set; }
        public Collection<string> TableNameCollection { get; private set; }
        public abstract void ExportToCsv(string xmlTableName, string csvDestinationFilePath, Encoding encoding);

        protected static string CreateCsvLine(IEnumerable<string> values, bool alwaysQuote)
        {
            return string.Join(",", values.Select(value => EscapeCsvValue(value, alwaysQuote)));
        }

        protected static string EscapeCsvValue(string value, bool alwaysQuote)
        {
            value = value ?? string.Empty;
            value = value
                .Replace("\r\n", @"\n")
                .Replace("\r", @"\n")
                .Replace("\n", @"\n");

            bool shouldQuote = alwaysQuote || value.Contains(",") || value.Contains("\"");
            value = value.Replace("\"", "\"\"");

            return shouldQuote ? "\"" + value + "\"" : value;
        }

        //protected static string CreateCsvRowHeader(Dictionary<int, string> headerColumnNames)
        //{
        //    const int colHeaderNr = 0;
        //    int columnCount = headerColumnNames.Count;
        //    string rowHeaderValue = string.Empty;

        //    foreach (KeyValuePair<int, string> keyValuePair in headerColumnNames)
        //    {
        //        rowHeaderValue += keyValuePair.Value;

        //        if (colHeaderNr < columnCount - 1)
        //        {
        //            rowHeaderValue += ",";
        //        }
        //    }

        //    return rowHeaderValue;
        //}
    }
}
