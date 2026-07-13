using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Timeless.DataConversion.XmlToCsv;

public sealed class XmlToCsvConverter : IDisposable
{
    private readonly bool _preferStreamingExport;
    private readonly List<string> _tableNames;
    private readonly string _xmlSourceFilePath;
    private bool _isDataLoaded;

    public XmlToCsvConverter(string xmlSourceFilePath, bool renameConflictingTables = false)
        : this(xmlSourceFilePath, renameConflictingTables, false)
    {

    }

    private XmlToCsvConverter(string xmlSourceFilePath, bool renameConflictingTables, bool preferStreamingExport)
    {
        _xmlSourceFilePath = xmlSourceFilePath;
        _preferStreamingExport = preferStreamingExport;
        _tableNames = new List<string>();
        DataSet = new DataSet();

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
    public IReadOnlyList<string> TableNames => _tableNames;

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

    public static XmlInferredTablePlan InferTablePlan(string xmlSourceFilePath)
    {
        XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlSourceFilePath);
        return new XmlTablePlanInferer().InferTables(profile);
    }

    public static XmlConversionPreview CreateConversionPreview(string xmlSourceFilePath)
    {
        XmlInferredTablePlan plan = InferTablePlan(xmlSourceFilePath);
        return new XmlConversionPreviewBuilder().Build(plan);
    }

    public static XmlInferredTablePlan ConfirmConversionPlan(XmlConversionPreview preview, XmlConversionPlanConfirmation confirmation)
    {
        if (preview == null)
        {
            throw new ArgumentNullException(nameof(preview));
        }

        if (confirmation == null)
        {
            throw new ArgumentNullException(nameof(confirmation));
        }

        var confirmationsByPath = confirmation.Tables.ToDictionary(item => item.Path);
        var confirmedTablesByPath = new Dictionary<string, XmlInferredTable>();

        foreach (XmlInferredTable sourceTable in preview.InferredPlan.Tables)
        {
            if (!confirmationsByPath.TryGetValue(sourceTable.Path, out XmlTablePlanConfirmation tableConfirmation) ||
                !tableConfirmation.Include)
            {
                continue;
            }

            XmlInferredTable confirmedTable = CreateConfirmedTable(sourceTable, tableConfirmation);
            confirmedTablesByPath.Add(confirmedTable.Path, confirmedTable);
        }

        foreach (XmlInferredTable sourceTable in preview.InferredPlan.Tables)
        {
            if (!confirmedTablesByPath.TryGetValue(sourceTable.Path, out XmlInferredTable confirmedTable))
            {
                continue;
            }

            foreach (XmlInferredTable sourceChildTable in sourceTable.ChildTables)
            {
                if (confirmedTablesByPath.TryGetValue(sourceChildTable.Path, out XmlInferredTable confirmedChildTable))
                {
                    confirmedTable.AddChildTable(confirmedChildTable);
                }
            }
        }

        return new XmlInferredTablePlan(preview.InferredPlan.Tables
            .Where(item => confirmedTablesByPath.ContainsKey(item.Path))
            .Select(item => confirmedTablesByPath[item.Path]));
    }

    public static void ExportInferredTables(string xmlSourceFilePath, string destinationDirectory, Encoding encoding)
    {
        XmlInferredTablePlan plan = InferTablePlan(xmlSourceFilePath);
        ExportInferredTables(xmlSourceFilePath, destinationDirectory, encoding, plan);
    }

    public static void ExportConfirmedConversion(
        string xmlSourceFilePath,
        string destinationDirectory,
        Encoding encoding,
        XmlConversionPreview preview,
        XmlConversionPlanConfirmation confirmation)
    {
        XmlInferredTablePlan confirmedPlan = ConfirmConversionPlan(preview, confirmation);
        ExportInferredTables(xmlSourceFilePath, destinationDirectory, encoding, confirmedPlan);
    }

    public static void ExportInferredTables(string xmlSourceFilePath, string destinationDirectory, Encoding encoding, XmlInferredTablePlan plan)
    {
        if (string.IsNullOrEmpty(destinationDirectory))
        {
            throw new NotSupportedException("Destination directory for inferred table export is not specified");
        }

        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        Directory.CreateDirectory(destinationDirectory);

        var exportState = new InferredTableExportState(plan, destinationDirectory, encoding);

        try
        {
            using XmlReader reader = XmlReader.Create(xmlSourceFilePath, CreateStreamingReaderSettings());
            var pathStack = new Stack<string>();

            while (!reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.None)
                {
                    if (!reader.Read())
                    {
                        break;
                    }

                    continue;
                }

                if (reader.NodeType != XmlNodeType.Element)
                {
                    if (reader.NodeType == XmlNodeType.EndElement && pathStack.Count > 0)
                    {
                        pathStack.Pop();
                    }

                    if (!reader.Read())
                    {
                        break;
                    }

                    continue;
                }

                string parentPath = pathStack.Count == 0 ? null : pathStack.Peek();
                string path = BuildXmlPath(parentPath, reader.LocalName);

                if (exportState.TryGetRootTable(path, out XmlInferredTable table))
                {
                    var row = (XElement)XNode.ReadFrom(reader);
                    exportState.WriteRow(table, row, null);
                    continue;
                }

                if (!reader.IsEmptyElement)
                {
                    pathStack.Push(path);
                }

                if (!reader.Read())
                {
                    break;
                }
            }
        }
        finally
        {
            exportState.Dispose();
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

    private static StreamWriter CreateStreamWriter(string destinationPath, Encoding encoding)
    {
        var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
        var sw = new StreamWriter(fs, encoding);
        return sw;
    }

    private static XmlInferredTable CreateConfirmedTable(XmlInferredTable sourceTable, XmlTablePlanConfirmation tableConfirmation)
    {
        var columnConfirmationsByPath = tableConfirmation.Columns.ToDictionary(item => item.Path);

        IEnumerable<XmlInferredColumn> columns = sourceTable.Columns
            .Where(column => !columnConfirmationsByPath.TryGetValue(column.Path, out XmlColumnPlanConfirmation columnConfirmation) ||
                             columnConfirmation.Include)
            .Select(column => CreateConfirmedColumn(column, columnConfirmationsByPath));

        string tableName = string.IsNullOrWhiteSpace(tableConfirmation.Name) ? sourceTable.Name : tableConfirmation.Name;

        return new XmlInferredTable(
            sourceTable.Path,
            tableName,
            sourceTable.RowCount,
            sourceTable.Score,
            columns,
            sourceTable.Reasons);
    }

    private static XmlInferredColumn CreateConfirmedColumn(
        XmlInferredColumn sourceColumn,
        Dictionary<string, XmlColumnPlanConfirmation> columnConfirmationsByPath)
    {
        if (columnConfirmationsByPath.TryGetValue(sourceColumn.Path, out XmlColumnPlanConfirmation columnConfirmation) &&
            !string.IsNullOrWhiteSpace(columnConfirmation.Name))
        {
            return new XmlInferredColumn(sourceColumn.Path, columnConfirmation.Name, sourceColumn.TypeHint);
        }

        return sourceColumn;
    }

    private bool TryExportUsingXmlReader(DataTable table, string destinationPath, Encoding encoding)
    {
        using XmlReader reader = XmlReader.Create(_xmlSourceFilePath, CreateStreamingReaderSettings());
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

            if (element is { HasElements: false })
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

    private static void WriteRowToCsv(StreamWriter sw, DataRow row, DataColumnCollection columns)
    {
        bool isFirstColumn = true;

        foreach (DataColumn column in columns)
        {
            WriteCsvField(sw, row[column].ToString(), true, isFirstColumn);
            isFirstColumn = false;
        }

        sw.WriteLine();
    }

    private static void WriteHeaderToCsv(StreamWriter sw, DataColumnCollection columns)
    {
        bool isFirstColumn = true;

        foreach (DataColumn column in columns)
        {
            WriteCsvField(sw, column.ColumnName, false, isFirstColumn);
            isFirstColumn = false;
        }

        sw.WriteLine();
    }

    private static void WriteInferredHeaderToCsv(StreamWriter sw, XmlInferredTable table, bool includeParentRowId)
    {
        WriteCsvField(sw, "_row_id", false, true);

        if (includeParentRowId)
        {
            WriteCsvField(sw, "_parent_row_id", false, false);
        }

        foreach (XmlInferredColumn column in table.Columns)
        {
            WriteCsvField(sw, column.Name, false, false);
        }

        sw.WriteLine();
    }

    private static string BuildXmlPath(string parentPath, string localName)
    {
        return string.IsNullOrEmpty(parentPath) ? "/" + localName : parentPath + "/" + localName;
    }

    private static string GetRelativeColumnValue(XElement row, string rowPath, string columnPath)
    {
        if (!columnPath.StartsWith(rowPath + "/", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        string relativePath = columnPath.Substring(rowPath.Length + 1);

        if (relativePath.StartsWith("@", StringComparison.Ordinal))
        {
            string attributeName = relativePath.Substring(1);
            return row.Attributes().FirstOrDefault(item => item.Name.LocalName == attributeName)?.Value ?? string.Empty;
        }

        XElement current = row;

        foreach (string segment in relativePath.Split('/'))
        {
            current = current.Elements().FirstOrDefault(item => item.Name.LocalName == segment);

            if (current == null)
            {
                return string.Empty;
            }
        }

        return current.HasElements ? string.Empty : current.Value;
    }

    private static XmlReaderSettings CreateStreamingReaderSettings()
    {
        return new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };
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

    private sealed class InferredTableExportState : IDisposable
    {
        private readonly Dictionary<string, StreamWriter> _writers;
        private readonly Dictionary<string, long> _rowIdsByTablePath;
        private readonly Dictionary<string, XmlInferredTable> _rootTablesByPath;
        private readonly Dictionary<string, bool> _includeParentRowIdByTablePath;

        public InferredTableExportState(XmlInferredTablePlan plan, string destinationDirectory, Encoding encoding)
        {
            _writers = new Dictionary<string, StreamWriter>();
            _rowIdsByTablePath = new Dictionary<string, long>();
            _rootTablesByPath = new Dictionary<string, XmlInferredTable>();
            _includeParentRowIdByTablePath = new Dictionary<string, bool>();

            var childTablePaths = new HashSet<string>(plan.Tables.SelectMany(table => table.ChildTables.Select(child => child.Path)));

            foreach (XmlInferredTable table in plan.Tables)
            {
                bool includeParentRowId = childTablePaths.Contains(table.Path);
                _includeParentRowIdByTablePath[table.Path] = includeParentRowId;

                string destinationPath = Path.Combine(destinationDirectory, table.Name + ".csv");
                StreamWriter writer = CreateStreamWriter(destinationPath, encoding);
                WriteInferredHeaderToCsv(writer, table, includeParentRowId);
                _writers.Add(table.Path, writer);
                _rowIdsByTablePath.Add(table.Path, 0);

                if (!childTablePaths.Contains(table.Path))
                {
                    _rootTablesByPath.Add(table.Path, table);
                }
            }
        }

        public bool TryGetRootTable(string path, out XmlInferredTable table)
        {
            return _rootTablesByPath.TryGetValue(path, out table);
        }

        public string WriteRow(XmlInferredTable table, XElement row, string parentRowId)
        {
            long nextId = _rowIdsByTablePath[table.Path] + 1;
            _rowIdsByTablePath[table.Path] = nextId;
            string rowId = nextId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            StreamWriter writer = _writers[table.Path];

            WriteCsvField(writer, rowId, true, true);

            if (_includeParentRowIdByTablePath[table.Path])
            {
                WriteCsvField(writer, parentRowId, true, false);
            }

            foreach (XmlInferredColumn column in table.Columns)
            {
                WriteCsvField(writer, GetRelativeColumnValue(row, table.Path, column.Path), true, false);
            }

            writer.WriteLine();

            foreach (XmlInferredTable childTable in table.ChildTables)
            {
                WriteChildRows(childTable, table.Path, row, rowId);
            }

            return rowId;
        }

        private void WriteChildRows(XmlInferredTable childTable, string parentTablePath, XElement parentRow, string parentRowId)
        {
            foreach (XElement childRow in FindDescendantRows(parentRow, parentTablePath, childTable.Path))
            {
                WriteRow(childTable, childRow, parentRowId);
            }
        }

        private static IEnumerable<XElement> FindDescendantRows(XElement parentRow, string parentTablePath, string childTablePath)
        {
            if (!childTablePath.StartsWith(parentTablePath + "/", StringComparison.Ordinal))
            {
                yield break;
            }

            string relativePath = childTablePath.Substring(parentTablePath.Length + 1);
            IEnumerable<XElement> current = new[] { parentRow };

            foreach (string segment in relativePath.Split('/'))
            {
                current = current.SelectMany(item => item.Elements().Where(element => element.Name.LocalName == segment));
            }

            foreach (XElement childRow in current)
            {
                yield return childRow;
            }
        }

        public void Dispose()
        {
            foreach (StreamWriter writer in _writers.Values)
            {
                writer.Dispose();
            }
        }
    }

    public void Dispose()
    {
        DataSet.Dispose();
    }
}
