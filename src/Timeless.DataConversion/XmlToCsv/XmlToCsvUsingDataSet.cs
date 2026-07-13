using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Timeless.DataConversion.XmlToCsv
{
    public class XmlToCsvUsingDataSet : IDisposable
    {
        private string _csvDestinationFilePath;
        private DataTable _workingTable;

        public XmlToCsvUsingDataSet(string xmlSourceFilePath)
            : this(xmlSourceFilePath, false)
        {

        }

        public XmlToCsvUsingDataSet(string xmlSourceFilePath, bool autoRenameWhenNamingConflict)
        {
            HeaderColumnNameCollection = new Dictionary<int, string>(64);
            TableNameCollection = new Collection<string>();
            XmlDataSet = new DataSet();
            try
            {
                XmlDataSet.ReadXml(xmlSourceFilePath);

                foreach (DataTable table in XmlDataSet.Tables)
                {
                    TableNameCollection.Add(table.TableName);
                }
            }
            catch (DuplicateNameException)
            {
                if (autoRenameWhenNamingConflict)
                {
                    XmlDataSet.ReadXml(xmlSourceFilePath, XmlReadMode.IgnoreSchema);

                    foreach (DataTable table in XmlDataSet.Tables)
                    {
                        TableNameCollection.Add(table.TableName);
                    }

                    RenameDuplicateColumn();
                }
                else
                {
                    throw;
                }
            }
        }

        public DataSet XmlDataSet { get; private set; }
        public Dictionary<int, string> HeaderColumnNameCollection { get; private set; }
        public Collection<string> TableNameCollection { get; private set; }
        private int ColumnCount { get; set; }

        /// <summary>
        /// Check for duplicates names in XML. Rename the table in case a clash with a column name is found.
        /// </summary>
        /// <returns>True if a duplicate XML name was found and renames the name clash. Otherwise returns false.</returns>
        private void RenameDuplicateColumn()
        {
            foreach (DataTable table in XmlDataSet.Tables)
            {
                bool hasDuplicate = XmlDataSet.Tables[0].Columns.Contains(table.TableName);

                if (hasDuplicate)
                {
                    TableNameCollection.Remove(table.TableName);
                    TableNameCollection.Add(table.TableName + "_Renamed");
                    table.TableName = table.TableName + "_Renamed";
                }
            }
        }

        public void ExportToCsv(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            StreamWriter sw = CreateStreamWriter(xmlTableName, csvDestinationFilePath, encoding);

            using (sw)
            {
                WriteHeaderToCsv(sw);

                foreach (DataRow row in XmlDataSet.Tables[xmlTableName].Rows)
                {
                    WriteRowToCsv(xmlTableName, sw, row);
                }

                sw.Flush();
                sw.Close();
            }
        }

        private StreamWriter CreateStreamWriter(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            if (string.IsNullOrEmpty(xmlTableName))
            {
                throw new NotSupportedException("Table name for table to export is not specified");
            }

            HeaderColumnNameCollection.Clear();

            _csvDestinationFilePath = csvDestinationFilePath;
            _workingTable = XmlDataSet.Tables[xmlTableName];
            ColumnCount = _workingTable.Columns.Count;

            foreach (DataColumn column in _workingTable.Columns)
            {
                HeaderColumnNameCollection.Add(column.Ordinal, column.ColumnName);
            }

            var fs = new FileStream(_csvDestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var sw = new StreamWriter(fs, encoding);
            return sw;
        }

        [Obsolete("Use XmlDataSet.Tables[xmlTableName].Columns instead.")]
        public List<DataColumn> GetColumnList(string xmlSourceFilePath, string xmlTableName)
        {
            //var ds = new DataSet("ds");
            //ds.ReadXml(xmlSourceFilePath);
            //var dt = XmlDataSet.Tables[xmlTableName];
            List<DataColumn> list = XmlDataSet.Tables[xmlTableName].Columns.Cast<DataColumn>().ToList();
            return list;
        }

        private void WriteRowToCsv(string xmlTableName, StreamWriter sw, DataRow row)
        {
            var columnValues = new List<string>(ColumnCount);

            foreach (DataColumn column in XmlDataSet.Tables[xmlTableName].Columns)
            {
                columnValues.Add(row[column].ToString());
            }

            sw.WriteLine(CreateCsvLine(columnValues, true));
        }

        private void WriteHeaderToCsv(StreamWriter sw)
        {
            sw.WriteLine(CreateCsvLine(HeaderColumnNameCollection.Values, false));
        }

        private static string CreateCsvLine(IEnumerable<string> values, bool alwaysQuote)
        {
            return string.Join(",", values.Select(value => EscapeCsvValue(value, alwaysQuote)));
        }

        private static string EscapeCsvValue(string value, bool alwaysQuote)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                XmlDataSet.Dispose();
            }
        }
    }
}
