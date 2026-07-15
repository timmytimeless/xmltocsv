using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

    public static XmlConversionPreview CreateConversionPreview(string xmlSourceFilePath, XmlConversionLimits limits)
    {
        var timer = Stopwatch.StartNew();
        var validator = new XmlConversionValidator();

        ThrowIfInvalid(validator.ValidateSourceFile(xmlSourceFilePath, limits));
        ThrowIfInvalid(validator.ValidateExecution(timer.Elapsed, limits));

        XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlSourceFilePath, limits, timer);
        ThrowIfInvalid(validator.ValidateStructuralProfile(profile, limits));
        ThrowIfInvalid(validator.ValidateExecution(timer.Elapsed, limits));

        XmlInferredTablePlan plan = new XmlTablePlanInferer().InferTables(profile);
        ThrowIfInvalid(validator.ValidateTablePlan(plan, limits));
        ThrowIfInvalid(validator.ValidateExecution(timer.Elapsed, limits));

        return new XmlConversionPreviewBuilder().Build(plan);
    }

    public static XmlConversionValidationResult ValidateSourceFile(string xmlSourceFilePath, XmlConversionLimits limits)
    {
        return new XmlConversionValidator().ValidateSourceFile(xmlSourceFilePath, limits);
    }

    public static XmlConversionValidationResult ValidateStructuralProfile(XmlStructuralProfile profile, XmlConversionLimits limits)
    {
        return new XmlConversionValidator().ValidateStructuralProfile(profile, limits);
    }

    public static XmlConversionValidationResult ValidateTablePlan(XmlInferredTablePlan plan, XmlConversionLimits limits)
    {
        return new XmlConversionValidator().ValidateTablePlan(plan, limits);
    }

    public static XmlConversionValidationResult ValidateOutputDirectory(string outputDirectory, XmlConversionLimits limits)
    {
        return new XmlConversionValidator().ValidateOutputDirectory(outputDirectory, limits);
    }

    public static XmlConversionValidationResult ValidateExecution(TimeSpan elapsed, XmlConversionLimits limits)
    {
        return new XmlConversionValidator().ValidateExecution(elapsed, limits);
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

    public static void ExportConfirmedConversion(
        string xmlSourceFilePath,
        string destinationDirectory,
        Encoding encoding,
        XmlConversionPreview preview,
        XmlConversionPlanConfirmation confirmation,
        XmlConversionLimits limits)
    {
        XmlInferredTablePlan confirmedPlan = ConfirmConversionPlan(preview, confirmation);
        ExportInferredTables(xmlSourceFilePath, destinationDirectory, encoding, confirmedPlan, limits);
    }

    public static void ExportInferredTables(string xmlSourceFilePath, string destinationDirectory, Encoding encoding, XmlInferredTablePlan plan)
    {
        ExportInferredTables(xmlSourceFilePath, destinationDirectory, encoding, plan, null, null);
    }

    private static void ExportInferredTables(
        string xmlSourceFilePath,
        string destinationDirectory,
        Encoding encoding,
        XmlInferredTablePlan plan,
        XmlConversionLimits limits,
        Stopwatch timer)
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
            var frameStack = new Stack<StreamingElementFrame>();

            while (!reader.EOF)
            {
                ThrowIfExecutionLimitExceeded(timer, limits);

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
                    if (reader.NodeType == XmlNodeType.Text ||
                        reader.NodeType == XmlNodeType.CDATA ||
                        reader.NodeType == XmlNodeType.SignificantWhitespace)
                    {
                        if (frameStack.Count > 0)
                        {
                            frameStack.Peek().Text.Append(reader.Value);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && frameStack.Count > 0)
                    {
                        CompleteStreamingElement(frameStack, exportState, limits);
                    }

                    if (!reader.Read())
                    {
                        break;
                    }

                    continue;
                }

                string parentPath = frameStack.Count == 0 ? null : frameStack.Peek().Path;
                string path = BuildXmlPath(parentPath, reader.LocalName);
                StreamingElementFrame frame = CreateStreamingElementFrame(reader, path, frameStack, exportState, limits);

                if (reader.IsEmptyElement)
                {
                    CompleteStreamingElement(frame, frameStack, exportState, limits);
                }
                else
                {
                    frameStack.Push(frame);
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

    public static void ExportInferredTables(
        string xmlSourceFilePath,
        string destinationDirectory,
        Encoding encoding,
        XmlInferredTablePlan plan,
        XmlConversionLimits limits)
    {
        if (string.IsNullOrEmpty(destinationDirectory))
        {
            throw new NotSupportedException("Destination directory for inferred table export is not specified");
        }

        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        var timer = Stopwatch.StartNew();
        var validator = new XmlConversionValidator();

        ThrowIfInvalid(validator.ValidateSourceFile(xmlSourceFilePath, limits));
        ThrowIfInvalid(validator.ValidateExecution(timer.Elapsed, limits));

        XmlStructuralProfile profile = new XmlStructuralProfiler().Profile(xmlSourceFilePath, limits, timer);
        ThrowIfInvalid(validator.ValidateStructuralProfile(profile, limits));
        ThrowIfInvalid(validator.ValidateExecution(timer.Elapsed, limits));

        ThrowIfInvalid(validator.ValidateTablePlan(plan, limits));
        ThrowIfInvalid(validator.ValidateExecution(timer.Elapsed, limits));

        ExportInferredTables(xmlSourceFilePath, destinationDirectory, encoding, plan, limits, timer);

        ThrowIfInvalid(validator.ValidateExecution(timer.Elapsed, limits));
        ThrowIfInvalid(validator.ValidateOutputDirectory(destinationDirectory, limits));
    }

    public static void ExportConfirmedConversionToZip(
        string xmlSourceFilePath,
        string destinationZipPath,
        Encoding encoding,
        XmlConversionPreview preview,
        XmlConversionPlanConfirmation confirmation,
        XmlConversionLimits limits)
    {
        XmlInferredTablePlan confirmedPlan = ConfirmConversionPlan(preview, confirmation);
        ExportInferredTablesToZip(xmlSourceFilePath, destinationZipPath, encoding, confirmedPlan, limits);
    }

    public static void ExportInferredTablesToZip(
        string xmlSourceFilePath,
        string destinationZipPath,
        Encoding encoding,
        XmlInferredTablePlan plan,
        XmlConversionLimits limits)
    {
        if (string.IsNullOrEmpty(destinationZipPath))
        {
            throw new NotSupportedException("Destination zip path for inferred table export is not specified");
        }

        string tempDirectory = Path.Combine(Path.GetTempPath(), "Timeless.DataConversion." + Guid.NewGuid().ToString("N"));

        try
        {
            ExportInferredTables(xmlSourceFilePath, tempDirectory, encoding, plan, limits);

            string parentDirectory = Path.GetDirectoryName(destinationZipPath);
            if (!string.IsNullOrEmpty(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            if (File.Exists(destinationZipPath))
            {
                File.Delete(destinationZipPath);
            }

            ZipFile.CreateFromDirectory(tempDirectory, destinationZipPath, CompressionLevel.Fastest, false);
            ThrowIfInvalid(new XmlConversionValidator().ValidateOutputZipFile(destinationZipPath, limits));
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }

    private static StreamingElementFrame CreateStreamingElementFrame(
        XmlReader reader,
        string path,
        Stack<StreamingElementFrame> frameStack,
        InferredTableExportState exportState,
        XmlConversionLimits limits)
    {
        if (frameStack.Count > 0)
        {
            frameStack.Peek().HasChildElements = true;
        }

        StreamingElementFrame parentTableFrame = frameStack.FirstOrDefault(item => item.Table != null);
        XmlInferredTable table = exportState.GetTable(path);
        string rowId = table == null ? null : exportState.ReserveRowId(table);
        var frame = new StreamingElementFrame(path, table, rowId, parentTableFrame?.RowId);

        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                string attributePath = path + "/@" + reader.LocalName;
                SetStreamingColumnValue(frameStack, frame, attributePath, reader.Value, limits);
            }

            reader.MoveToElement();
        }

        return frame;
    }

    private static void CompleteStreamingElement(
        Stack<StreamingElementFrame> frameStack,
        InferredTableExportState exportState,
        XmlConversionLimits limits)
    {
        StreamingElementFrame frame = frameStack.Pop();
        CompleteStreamingElement(frame, frameStack, exportState, limits);
    }

    private static void CompleteStreamingElement(
        StreamingElementFrame frame,
        Stack<StreamingElementFrame> frameStack,
        InferredTableExportState exportState,
        XmlConversionLimits limits)
    {
        if (!frame.HasChildElements)
        {
            SetStreamingColumnValue(frameStack, frame, frame.Path, frame.Text.ToString(), limits);
        }

        if (frame.Table != null)
        {
            exportState.WriteRow(frame.Table, frame.RowId, frame.ParentRowId, frame.Values);
        }
    }

    private static void SetStreamingColumnValue(
        Stack<StreamingElementFrame> frameStack,
        StreamingElementFrame currentFrame,
        string columnPath,
        string value,
        XmlConversionLimits limits)
    {
        if (currentFrame.Table != null && currentFrame.AcceptsColumn(columnPath))
        {
            currentFrame.SetValue(columnPath, value, limits);
        }

        foreach (StreamingElementFrame frame in frameStack)
        {
            if (frame.Table != null && frame.AcceptsColumn(columnPath))
            {
                frame.SetValue(columnPath, value, limits);
                return;
            }
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

    private static void ThrowIfInvalid(XmlConversionValidationResult result)
    {
        if (!result.IsValid)
        {
            throw new XmlConversionValidationException(result);
        }
    }

    private static void ThrowIfExecutionLimitExceeded(Stopwatch timer, XmlConversionLimits limits)
    {
        ThrowIfInvalid(new XmlConversionValidator().ValidateExecution(timer?.Elapsed ?? TimeSpan.Zero, limits));
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

    private sealed class StreamingElementFrame
    {
        private readonly HashSet<string> _columnPaths;

        public StreamingElementFrame(string path, XmlInferredTable table, string rowId, string parentRowId)
        {
            Path = path;
            Table = table;
            RowId = rowId;
            ParentRowId = parentRowId;
            Text = new StringBuilder();
            Values = new Dictionary<string, string>();
            _columnPaths = table == null
                ? new HashSet<string>()
                : new HashSet<string>(table.Columns.Select(column => column.Path));
        }

        public string Path { get; }
        public XmlInferredTable Table { get; }
        public string RowId { get; }
        public string ParentRowId { get; }
        public bool HasChildElements { get; set; }
        public StringBuilder Text { get; }
        public Dictionary<string, string> Values { get; }

        public bool AcceptsColumn(string columnPath)
        {
            return _columnPaths.Contains(columnPath);
        }

        public void SetValue(string columnPath, string value, XmlConversionLimits limits)
        {
            Values.TryGetValue(columnPath, out string existingValue);
            RowValueBytes -= Encoding.UTF8.GetByteCount(existingValue ?? string.Empty);
            RowValueBytes += Encoding.UTF8.GetByteCount(value ?? string.Empty);

            if (limits?.MaxRowSubtreeBytes is long maxRowSubtreeBytes && RowValueBytes > maxRowSubtreeBytes)
            {
                ThrowIfInvalid(new XmlConversionValidationResult(new[]
                {
                    XmlConversionValidationIssue.Create("max_row_subtree_bytes", "Streamed row value size in bytes", RowValueBytes, maxRowSubtreeBytes)
                }));
            }

            Values[columnPath] = value;
        }

        private long RowValueBytes { get; set; }
    }

    private sealed class InferredTableExportState : IDisposable
    {
        private readonly Dictionary<string, StreamWriter> _writers;
        private readonly Dictionary<string, long> _rowIdsByTablePath;
        private readonly Dictionary<string, XmlInferredTable> _tablesByPath;
        private readonly Dictionary<string, bool> _includeParentRowIdByTablePath;

        public InferredTableExportState(XmlInferredTablePlan plan, string destinationDirectory, Encoding encoding)
        {
            _writers = new Dictionary<string, StreamWriter>();
            _rowIdsByTablePath = new Dictionary<string, long>();
            _tablesByPath = new Dictionary<string, XmlInferredTable>();
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
                _tablesByPath.Add(table.Path, table);
            }
        }

        public XmlInferredTable GetTable(string path)
        {
            _tablesByPath.TryGetValue(path, out XmlInferredTable table);
            return table;
        }

        public string ReserveRowId(XmlInferredTable table)
        {
            long nextId = _rowIdsByTablePath[table.Path] + 1;
            _rowIdsByTablePath[table.Path] = nextId;
            return nextId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public void WriteRow(XmlInferredTable table, string rowId, string parentRowId, IReadOnlyDictionary<string, string> values)
        {
            StreamWriter writer = _writers[table.Path];

            WriteCsvField(writer, rowId, true, true);

            if (_includeParentRowIdByTablePath[table.Path])
            {
                WriteCsvField(writer, parentRowId, true, false);
            }

            foreach (XmlInferredColumn column in table.Columns)
            {
                values.TryGetValue(column.Path, out string value);
                WriteCsvField(writer, value, true, false);
            }

            writer.WriteLine();
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
