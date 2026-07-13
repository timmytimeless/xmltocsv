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
        public XmlToCsvUsingDataSet(string xmlSourceFilePath)
            : this(xmlSourceFilePath, false)
        {

        }

        public XmlToCsvUsingDataSet(string xmlSourceFilePath, bool autoRenameWhenNamingConflict)
        {
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
