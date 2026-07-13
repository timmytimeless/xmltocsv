using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using Timeless.DataConversion.XmlToCsvHelpers;

namespace Timeless.DataConversion.XmlToCsvStrategy
{
    public class XmlToCsvUsingLinq : XmlToCsvStrategyBase
    {
        private string _csvDestinationFilePath;
        private readonly string _xmlSourceFilePath;

        public XmlToCsvUsingLinq(string xmlSourceFilePath)
        {
            _xmlSourceFilePath = @xmlSourceFilePath;

            using var ds = new DataSet("ds");
            
            ds.ReadXmlSchema(@_xmlSourceFilePath);

            foreach (DataTable table in ds.Tables)
            {
                TableNameCollection.Add(table.TableName);
            }
        }

        public override void ExportToCsv(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            _csvDestinationFilePath = csvDestinationFilePath;

            HeaderColumnNameCollection.Clear();
            ColumnCount = 0;

            using var reader = XmlReader.Create(_xmlSourceFilePath);
            
            var workingTable =
                from el in reader.StreamElements(xmlTableName).DescendantsAndSelf()
                where el.Descendants().Any()
                select el;

            IEnumerable<XElement> list = workingTable.ToList();

            using var fs = new FileStream(_csvDestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var sw = new StreamWriter(fs, encoding);

            foreach (var x in list.Take(1).Descendants())
            {
                HeaderColumnNameCollection.Add(ColumnCount, x.Name.ToString());
                ColumnCount++;
            }

            sw.WriteLine(CreateCsvLine(HeaderColumnNameCollection.Values, false));

            foreach (var element in list)
            {
                sw.WriteLine(CreateCsvLine(element.Descendants().Select(obj => obj.Value), true));
            }
        }
    }
}
