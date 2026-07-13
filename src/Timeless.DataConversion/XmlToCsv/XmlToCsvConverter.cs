using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Timeless.DataConversion.XmlToCsv
{
    public sealed class XmlToCsvConverter : IDisposable
    {
        private readonly bool _preferStreamingExport;
        private readonly List<string> _tableNames;
        private readonly string _xmlSourceFilePath;
        private bool _isDataLoaded;

        public XmlToCsvConverter(string xmlSourceFilePath)
            : this(xmlSourceFilePath, false)
        {

        }

        public XmlToCsvConverter(string xmlSourceFilePath, bool renameConflictingTables)
            : this(xmlSourceFilePath, renameConflictingTables, false)
        {

        }

        private XmlToCsvConverter(string xmlSourceFilePath, bool renameConflictingTables, bool preferStreamingExport)
        {
            _xmlSourceFilePath = xmlSourceFilePath;
            _preferStreamingExport = preferStreamingExport;
            _tableNames = new List<string>();
            DataSet = new System.Data.DataSet();

            if (preferStreamingExport)
            {
                DataSet.InferXmlSchema(xmlSourceFilePath, null);
                PopulateTableNames();
                return;
            }

            LoadData(xmlSourceFilePath, renameConflictingTables);
        }

        public static XmlToCsvConverter CreateStreaming(string xmlSourceFilePath)
        {
            return new XmlToCsvConverter(xmlSourceFilePath, false, true);
        }

        private void LoadData(string xmlSourceFilePath, bool renameConflictingTables)
        {
            _tableNames.Clear();

            try
            {
                DataSet.ReadXml(xmlSourceFilePath);
                _isDataLoaded = true;
                PopulateTableNames();
            }
            catch (DuplicateNameException)
            {
                if (renameConflictingTables)
                {
                    DataSet.ReadXml(xmlSourceFilePath, XmlReadMode.IgnoreSchema);
                    _isDataLoaded = true;

                    PopulateTableNames();
                    RenameDuplicateColumn();
                }
                else
                {
                    throw;
                }
            }
        }

        private void PopulateTableNames()
        {
            foreach (DataTable table in DataSet.Tables)
            {
                _tableNames.Add(table.TableName);
            }
        }

        internal DataSet DataSet { get; private set; }
        public IReadOnlyList<string> TableNames
        {
            get { return _tableNames; }
        }

        /// <summary>
        /// Check for duplicates names in XML. Rename the table in case a clash with a column name is found.
        /// </summary>
        /// <returns>True if a duplicate XML name was found and renames the name clash. Otherwise returns false.</returns>
        private void RenameDuplicateColumn()
        {
            foreach (DataTable table in DataSet.Tables)
            {
                bool hasDuplicate = DataSet.Tables[0].Columns.Contains(table.TableName);

                if (hasDuplicate)
                {
                    _tableNames.Remove(table.TableName);
                    _tableNames.Add(table.TableName + "_Renamed");
                    table.TableName = table.TableName + "_Renamed";
                }
            }
        }

        public void Export(string tableName, string destinationPath, Encoding encoding)
        {
            DataTable table = GetTableToExport(tableName);

            if (_preferStreamingExport && TryExportUsingXmlReader(table, destinationPath, encoding))
            {
                return;
            }

            EnsureDataLoaded();
            table = GetTableToExport(tableName);
            StreamWriter sw = CreateStreamWriter(destinationPath, encoding);

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

            DataSet.Clear();
            LoadData(_xmlSourceFilePath, false);
        }

        private DataTable GetTableToExport(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new NotSupportedException("Table name for table to export is not specified");
            }

            return DataSet.Tables[tableName];
        }

        private StreamWriter CreateStreamWriter(string destinationPath, Encoding encoding)
        {
            var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            var sw = new StreamWriter(fs, encoding);
            return sw;
        }

        private bool TryExportUsingXmlReader(DataTable table, string destinationPath, Encoding encoding)
        {
            using XmlReader reader = XmlReader.Create(_xmlSourceFilePath);
            StreamWriter sw = null;
            ColumnBinding[] bindings = null;

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

                    sw = CreateStreamWriter(destinationPath, encoding);
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

        private static ColumnBinding[] CreateColumnBindings(DataColumnCollection columns, XElement row)
        {
            var bindings = new ColumnBinding[columns.Count];

            for (int i = 0; i < columns.Count; i++)
            {
                string columnName = columns[i].ColumnName;
                XAttribute attribute = row.Attributes().FirstOrDefault(item => item.Name.LocalName == columnName);

                if (attribute != null)
                {
                    bindings[i] = new ColumnBinding(columnName, true);
                    continue;
                }

                XElement element = row.Elements().FirstOrDefault(item => item.Name.LocalName == columnName);

                if (element != null && !element.HasElements)
                {
                    bindings[i] = new ColumnBinding(columnName, false);
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

        private static void WriteStreamingRow(StreamWriter sw, ColumnBinding[] bindings, XElement row)
        {
            for (int i = 0; i < bindings.Length; i++)
            {
                WriteCsvField(sw, bindings[i].GetValue(row), true, i == 0);
            }

            sw.WriteLine();
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

        private sealed class ColumnBinding
        {
            private readonly string _name;
            private readonly bool _isAttribute;

            internal ColumnBinding(string name, bool isAttribute)
            {
                _name = name;
                _isAttribute = isAttribute;
            }

            internal string GetValue(XElement row)
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
            DataSet.Dispose();
        }
    }
}
