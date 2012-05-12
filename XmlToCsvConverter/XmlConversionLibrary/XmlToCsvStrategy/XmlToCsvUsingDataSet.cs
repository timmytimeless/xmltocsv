using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Moor.XmlConversionLibrary.XmlToCsvStrategy
{
    public class XmlToCsvUsingDataSet : XmlToCsvStrategyBase, IDisposable
    {
        private readonly DataSet _xmlDataSet = new DataSet();
        private string _csvDestinationFilePath;
        private DataTable _workingTable;


        public XmlToCsvUsingDataSet(string xmlSourceFilePath)
            : this(xmlSourceFilePath, false) 
        {

        }

 
        public XmlToCsvUsingDataSet(string xmlSourceFilePath, bool renameTablesWhenDuplicateNamesExist) 
        {
            _xmlDataSet.ReadXml(xmlSourceFilePath);

            foreach (DataTable table in _xmlDataSet.Tables)
            {
                TableNameCollection.Add(table.TableName);
            }
        }

        public override void ExportToCsv(string xmlTableName, string csvDestinationFilePath, Encoding encoding)
        {
            StreamWriter sw = CreateStreamWriter(xmlTableName, csvDestinationFilePath, encoding);

            using (sw)
            {
                WriteHeaderToCsv(sw);

                foreach (DataRow row in _xmlDataSet.Tables[xmlTableName].Rows)
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
            _workingTable = _xmlDataSet.Tables[xmlTableName];
            ColumnCount = _workingTable.Columns.Count;

            foreach (DataColumn column in _workingTable.Columns)
            {
                HeaderColumnNameCollection.Add(column.Ordinal, column.ColumnName);
            }

            var fs = new FileStream(_csvDestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var sw = new StreamWriter(fs, encoding);
            return sw;
        }

        public static List<DataColumn> GetColumnList(string xmlSourceFilePath, string xmlTableName)
        {
            var list = new List<DataColumn>();
   
            var ds = new DataSet("ds");
            ds.ReadXml(xmlSourceFilePath);
            var dt = ds.Tables[xmlTableName];

            foreach (DataColumn column in dt.Columns)
            {
                list.Add(column);
            }

            return list;
        }

        private void WriteRowToCsv(string xmlTableName, StreamWriter sw, DataRow row)
        {
            int colNr = 0;

            string rowValue = string.Empty;

            foreach (DataColumn column in _xmlDataSet.Tables[xmlTableName].Columns)
            {
                bool isString = (column.DataType == typeof(string));
                string columnValue;

                if (isString)
                {
                    string stringValue = row[column].ToString();
                    stringValue = stringValue.Replace(Environment.NewLine, @"\n");
                    columnValue = "\"" + stringValue + "\"";
                }
                else
                {
                    columnValue = row[column].ToString();
                }

                rowValue += columnValue;

                if (colNr < ColumnCount - 1)
                {
                    rowValue += ",";
                }

                colNr++;
            }

            sw.WriteLine(rowValue);
        }

        private void WriteHeaderToCsv(StreamWriter sw)
        {
            string headerLine = string.Empty;

            foreach (KeyValuePair<int, string> pair in HeaderColumnNameCollection)
            {
                headerLine += pair.Value + ",";
            }

            char[] charsToTrim = { ',' };
            sw.WriteLine(headerLine.TrimEnd(charsToTrim));
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
                _xmlDataSet.Dispose();
            }
        }
    }
}