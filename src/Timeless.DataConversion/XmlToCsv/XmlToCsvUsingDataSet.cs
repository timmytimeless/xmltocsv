using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Timeless.DataConversion.XmlToCsv
{
    public class XmlToCsvUsingDataSet : IDisposable
    {
        private readonly bool _preferStreamingExport;
        private readonly string _xmlSourceFilePath;
        private bool _isDataLoaded;

        public XmlToCsvUsingDataSet(string xmlSourceFilePath)
            : this(xmlSourceFilePath, false)
        {

        }

        public XmlToCsvUsingDataSet(string xmlSourceFilePath, bool autoRenameWhenNamingConflict)
            : this(xmlSourceFilePath, autoRenameWhenNamingConflict, false)
        {

        }

        private XmlToCsvUsingDataSet(string xmlSourceFilePath, bool autoRenameWhenNamingConflict, bool preferStreamingExport)
        {
            _xmlSourceFilePath = xmlSourceFilePath;
            _preferStreamingExport = preferStreamingExport;
            TableNameCollection = new Collection<string>();
            XmlDataSet = new DataSet();

            if (preferStreamingExport)
            {
                XmlDataSet.InferXmlSchema(xmlSourceFilePath, null);
                PopulateTableNameCollection();
                return;
            }

            LoadData(xmlSourceFilePath, autoRenameWhenNamingConflict);
        }

        public static XmlToCsvUsingDataSet CreateForStreamingExport(string xmlSourceFilePath)
        {
            return new XmlToCsvUsingDataSet(xmlSourceFilePath, false, true);
        }

        private void LoadData(string xmlSourceFilePath, bool autoRenameWhenNamingConflict)
        {
            TableNameCollection.Clear();

            try
            {
                XmlDataSet.ReadXml(xmlSourceFilePath);
                _isDataLoaded = true;
                PopulateTableNameCollection();
            }
            catch (DuplicateNameException)
            {
                if (autoRenameWhenNamingConflict)
                {
                    XmlDataSet.ReadXml(xmlSourceFilePath, XmlReadMode.IgnoreSchema);
                    _isDataLoaded = true;

                    PopulateTableNameCollection();
                    RenameDuplicateColumn();
                }
                else
                {
                    throw;
                }
            }
        }

        private void PopulateTableNameCollection()
        {
            foreach (DataTable table in XmlDataSet.Tables)
            {
                TableNameCollection.Add(table.TableName);
            }
        }

        public DataSet XmlDataSet { get; private set; }
        public Collection<string> TableNameCollection { get; private set; }

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
            DataTable table = GetTableToExport(xmlTableName);

            if (_preferStreamingExport && TryExportToCsvUsingXmlReader(table, csvDestinationFilePath, encoding))
            {
                return;
            }

            EnsureDataLoaded();
            table = GetTableToExport(xmlTableName);
            StreamWriter sw = CreateStreamWriter(csvDestinationFilePath, encoding);

            using (sw)
            {
                WriteHeaderToCsv(sw, table.Columns);

                foreach (DataRow row in table.Rows)
                {
                    WriteRowToCsv(sw, row, table.Columns);
                }

                sw.Flush();
                sw.Close();
            }
        }

        private void EnsureDataLoaded()
        {
            if (_isDataLoaded)
            {
                return;
            }

            XmlDataSet.Clear();
            LoadData(_xmlSourceFilePath, false);
        }

        private DataTable GetTableToExport(string xmlTableName)
        {
            if (string.IsNullOrEmpty(xmlTableName))
            {
                throw new NotSupportedException("Table name for table to export is not specified");
            }

            return XmlDataSet.Tables[xmlTableName];
        }

        private StreamWriter CreateStreamWriter(string csvDestinationFilePath, Encoding encoding)
        {
            var fs = new FileStream(csvDestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var sw = new StreamWriter(fs, encoding);
            return sw;
        }

        private bool TryExportToCsvUsingXmlReader(DataTable table, string csvDestinationFilePath, Encoding encoding)
        {
            using XmlReader reader = XmlReader.Create(_xmlSourceFilePath);
            StreamWriter sw = null;
            CsvColumnBinding[] bindings = null;

            while (!reader.EOF)
            {
                if (reader.NodeType != XmlNodeType.Element || reader.LocalName != table.TableName)
                {
                    if (!reader.Read())
                    {
                        break;
                    }

                    continue;
                }

                var row = (XElement)XNode.ReadFrom(reader);

                if (bindings == null)
                {
                    bindings = CreateColumnBindings(table.Columns, row);

                    if (bindings == null)
                    {
                        return false;
                    }

                    sw = CreateStreamWriter(csvDestinationFilePath, encoding);
                    WriteHeaderToCsv(sw, table.Columns);
                }

                if (HasNestedElement(row))
                {
                    sw?.Dispose();
                    return false;
                }

                WriteStreamingRow(sw, bindings, row);
            }

            sw?.Dispose();
            return bindings != null;
        }

        private static CsvColumnBinding[] CreateColumnBindings(DataColumnCollection columns, XElement row)
        {
            var bindings = new CsvColumnBinding[columns.Count];

            for (int i = 0; i < columns.Count; i++)
            {
                string columnName = columns[i].ColumnName;
                XAttribute attribute = row.Attributes().FirstOrDefault(item => item.Name.LocalName == columnName);

                if (attribute != null)
                {
                    bindings[i] = new CsvColumnBinding(columnName, true);
                    continue;
                }

                XElement element = row.Elements().FirstOrDefault(item => item.Name.LocalName == columnName);

                if (element != null && !element.HasElements)
                {
                    bindings[i] = new CsvColumnBinding(columnName, false);
                    continue;
                }

                return null;
            }

            return bindings;
        }

        private static bool HasNestedElement(XElement row)
        {
            return row.Elements().Any(element => element.HasElements);
        }

        private static void WriteStreamingRow(StreamWriter sw, CsvColumnBinding[] bindings, XElement row)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                WriteCsvField(sw, bindings[i].GetValue(row), true, i == 0);
            }

            sw.WriteLine();
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

        private void WriteRowToCsv(StreamWriter sw, DataRow row, DataColumnCollection columns)
        {
            bool isFirstColumn = true;

            foreach (DataColumn column in columns)
            {
                WriteCsvField(sw, row[column].ToString(), true, isFirstColumn);
                isFirstColumn = false;
            }

            sw.WriteLine();
        }

        private void WriteHeaderToCsv(StreamWriter sw, DataColumnCollection columns)
        {
            bool isFirstColumn = true;

            foreach (DataColumn column in columns)
            {
                WriteCsvField(sw, column.ColumnName, false, isFirstColumn);
                isFirstColumn = false;
            }

            sw.WriteLine();
        }

        private static void WriteCsvField(TextWriter writer, string value, bool alwaysQuote, bool isFirstColumn)
        {
            if (!isFirstColumn)
            {
                writer.Write(',');
            }

            value = value ?? string.Empty;
            bool shouldQuote = alwaysQuote || ContainsCsvSpecialCharacter(value);

            if (shouldQuote)
            {
                writer.Write('"');
            }

            WriteEscapedCsvValue(writer, value);

            if (shouldQuote)
            {
                writer.Write('"');
            }
        }

        private static bool ContainsCsvSpecialCharacter(string value)
        {
            return value.IndexOf(',') >= 0 || value.IndexOf('"') >= 0;
        }

        private static void WriteEscapedCsvValue(TextWriter writer, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];

                if (current == '\r')
                {
                    writer.Write(@"\n");

                    if (i + 1 < value.Length && value[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else if (current == '\n')
                {
                    writer.Write(@"\n");
                }
                else if (current == '"')
                {
                    writer.Write("\"\"");
                }
                else
                {
                    writer.Write(current);
                }
            }
        }

        private sealed class CsvColumnBinding
        {
            private readonly string _name;
            private readonly bool _isAttribute;

            public CsvColumnBinding(string name, bool isAttribute)
            {
                _name = name;
                _isAttribute = isAttribute;
            }

            public string GetValue(XElement row)
            {
                if (_isAttribute)
                {
                    return row.Attributes().FirstOrDefault(item => item.Name.LocalName == _name)?.Value ?? string.Empty;
                }

                return row.Elements().FirstOrDefault(item => item.Name.LocalName == _name)?.Value ?? string.Empty;
            }
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
