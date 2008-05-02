using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using Moor.XmlConversionLibrary.XmlToCsvHelpers;

namespace Moor.XmlConversionLibrary.XmlToCsvStrategy
{
    public class XmlToCsvUsingLinq : XmlToCsvStrategyBase
    {
        private string _csvDestinationFilePath;
        private readonly string _xmlSourceFilePath;

        public XmlToCsvUsingLinq(string xmlSourceFilePath)
        {
            _xmlSourceFilePath = @xmlSourceFilePath;

            DataSet ds = new DataSet("idpoorDataSet");
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

            _csvDestinationFilePath = csvDestinationFilePath;

            using (XmlReader reader = XmlReader.Create(_xmlSourceFilePath))
            {
                IEnumerable<XElement> _workingTable =
                    from el in reader.StreamElements(xmlTableName).DescendantsAndSelf()
                    where el.Descendants().Count() > 0
                    select el;

                IEnumerable<XElement> list = _workingTable.ToList();

                FileStream fs = new FileStream(_csvDestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);

                string headerLine = string.Empty;
                IEnumerable<XElement> data = list;

                foreach (XElement x in list.Take(1).Descendants())
                {
                    HeaderColumnNameCollection.Add(ColumnCount, x.Name.ToString());
                    headerLine += x.Name + ",";
                    ColumnCount++;
                }

                using (sw)
                {
                    
                    char[] charsToTrim = { ',' };
                    sw.WriteLine(headerLine.TrimEnd(charsToTrim));

                    foreach (XElement element in list)
                    {
                        string rowString = string.Empty;
                        string columnString = string.Empty;

                        foreach (var obj in element.Descendants())
                        {
                            columnString += obj.Value + ",";
                        }

                        rowString += columnString;
                        rowString = rowString.Replace(Environment.NewLine, @"-");
                        sw.WriteLine(rowString.TrimEnd(charsToTrim));
                    }

                    sw.Close();
                }

                reader.Close();
            }

            //DataSet ds = new DataSet("idpoorDataSet");
            //ds.ReadXml(@_xmlSourceFilePath);

            //DataRow row = ds.Tables["DataCollectionRound"].Rows[0];

            //foreach (DataColumn col in row.Table.Columns)
            //{
            //    Console.WriteLine(col.ColumnName);
            //}

        }


    }
}