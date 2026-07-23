# XML to CSV Converter

Timeless.DataConversion is a .NET 10 solution for converting XML data into one or more CSV files. It contains a reusable NuGet-packable conversion library, a command line tool, and NUnit tests with sample XML files.

NuGet package: [Timeless.DataConversion](https://www.nuget.org/packages/Timeless.DataConversion/)

For one-off conversions, you may use [tidelose.com](https://www.tidelose.com), which is based on this project and its NuGet package.

The legacy conversion path uses `DataSet` schema inference to discover tables in an XML document and export each discovered table to a separate CSV file. It remains available for callers that need the original behavior.

The public conversion path profiles XML with `XmlReader`, infers candidate tables, builds a conversion preview, allows callers to confirm or rename tables and columns, and exports inferred tables as related CSV files with generated row IDs. The console application uses this public workflow.

## Solution Structure

| Path | Purpose |
| --- | --- |
| `src/Timeless.DataConversion` | Core conversion library. Contains the XML-to-CSV converter. |
| `src/Timeless.DataConversion.Console` | Command line application for converting an XML file to CSV files in an output directory. |
| `tests/Timeless.DataConversion.Tests` | NUnit test project with XML fixtures for conversion behavior and known error cases. |
| `docs/ChangeLog.txt` | Historical change log. |
| `docs/PublicXmlToCsvConversionSpecification.md` | Design notes for the streaming, preview-driven XML-to-CSV workflow. |

## Features

- Converts XML tables to CSV files.
- Exports each discovered XML table as a separate CSV file.
- Profiles XML structure with a streaming reader for preview-driven conversion workflows.
- Infers candidate parent and child tables from repeated XML paths.
- Scores inferred tables with repeated-path, leaf-column, row-count, and structural-signature signals.
- Creates conversion previews with tables, columns, child table relationships, inference scores, reasons, and warnings.
- Allows callers to confirm, exclude, and rename inferred tables and columns before export.
- Exports inferred nested tables with `_row_id` and `_parent_row_id` columns.
- Exports confirmed conversions to a zip archive.
- Supports configurable conversion limits for file size, XML depth, unique paths, generated CSV count, column count, output directory size, output zip size, row subtree size, timeout, and cancellation.
- Provides a public conversion service facade for hosted or GUI workflows that should reject ambiguous inferred plans before export.
- Escapes double quotes in string values for CSV output.
- Replaces line breaks inside string values before writing CSV rows.
- Detects duplicate XML table and column naming conflicts.
- Can optionally auto-rename conflicting table names.
- Includes tests for basic conversion, attributes, double quote escaping, nested XML errors, duplicate name conflicts, preview generation, inferred table export, performance, and limit enforcement.

## Requirements

- .NET 10 SDK.
- Visual Studio 2026, Rider, or another IDE with .NET 10 SDK support.
- NuGet package restore access for test dependencies.

## Build

Open `Timeless.DataConversion.sln` in Rider or Visual Studio and build the solution.

From the command line, restore and build with the .NET SDK:

```sh
dotnet restore Timeless.DataConversion.sln
dotnet build Timeless.DataConversion.sln --configuration Release
```

Create the library NuGet package with:

```sh
dotnet pack src/Timeless.DataConversion/Timeless.DataConversion.csproj --configuration Release
```

## Command Line Usage

After building `Timeless.DataConversion.Console`, export CSV files to a directory with:

```bat
Timeless.DataConversion.Console.exe -xml C:\path\input.xml -dir C:\path\output -encoding utf-8
```

To produce a zip archive instead, pass `-zip`:

```bat
Timeless.DataConversion.Console.exe -xml C:\path\input.xml -zip C:\path\output.zip -encoding utf-8
```

Arguments:

| Argument | Description |
| --- | --- |
| `-xml` | Path to the source XML file. |
| `-dir` | Directory where generated CSV files should be written. Required unless `-zip` is supplied. |
| `-zip` | Optional zip archive path for the generated CSV files. |
| `-encoding` | Optional CSV output encoding. Defaults to `unicode`. Example: `utf-8`. |
| `-help` | Prints command line help. |

The console application writes one CSV file per inferred XML table using the table name as the file name. Nested tables are exported as related CSV files with `_row_id` and `_parent_row_id`.

## Library Usage

Use `XmlToCsvConverter` for the legacy DataSet-backed conversion implementation:

```csharp
using System.Text;
using Timeless.DataConversion.XmlToCsv;

using var converter = new XmlToCsvConverter(@"C:\path\input.xml");

foreach (string tableName in converter.TableNames)
{
    converter.Export(tableName, @"C:\path\output\" + tableName + ".csv", Encoding.UTF8);
}
```

To allow automatic renaming when the XML has duplicate table and column names:

```csharp
var converter = new XmlToCsvConverter(@"C:\path\input.xml", true);
```

For flat XML where the schema can be inferred up front, `CreateStreaming` avoids loading all row data before export when the table shape is supported:

```csharp
using var converter = XmlToCsvConverter.CreateStreaming(@"C:\path\input.xml");

foreach (string tableName in converter.TableNames)
{
    converter.Export(tableName, @"C:\path\output\" + tableName + ".csv", Encoding.UTF8);
}
```

For nested or arbitrary XML, use the preview-driven APIs:

```csharp
XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(@"C:\path\input.xml");

XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);

XmlTablePlanConfirmation firstTable = confirmation.Tables[0];
firstTable.Name = "orders";

XmlColumnPlanConfirmation firstColumn = firstTable.Columns[0];
firstColumn.Name = "order_number";

XmlToCsvConverter.ExportConfirmedConversion(
    @"C:\path\input.xml",
    @"C:\path\output",
    Encoding.UTF8,
    preview,
    confirmation);
```

You can also export the inferred plan directly without a confirmation step:

```csharp
XmlToCsvConverter.ExportInferredTables(@"C:\path\input.xml", @"C:\path\output", Encoding.UTF8);
```

For hosted or user-facing workflows, pass `XmlConversionLimits` to validation-aware methods:

```csharp
var limits = new XmlConversionLimits
{
    MaxFileSizeBytes = 100 * 1024 * 1024,
    MaxXmlDepth = 64,
    MaxUniquePaths = 10_000,
    MaxColumnsPerTable = 250,
    MaxGeneratedCsvFiles = 50,
    MaxOutputBytes = 250 * 1024 * 1024,
    MaxOutputZipBytes = 250 * 1024 * 1024,
    MaxRowSubtreeBytes = 5 * 1024 * 1024,
    Timeout = TimeSpan.FromMinutes(2)
};

XmlConversionPreview preview = XmlToCsvConverter.CreateConversionPreview(@"C:\path\input.xml", limits);
XmlToCsvConverter.ExportInferredTables(@"C:\path\input.xml", @"C:\path\output", Encoding.UTF8, preview.InferredPlan, limits);
```

Use `XmlPublicConversionService` as the API entry point for future website or GUI workflows. It enables ambiguous-plan rejection and exposes preview, confirmation, directory export, and zip export operations:

```csharp
var service = new XmlPublicConversionService(limits);
XmlConversionPreview preview = service.CreatePreview(@"C:\path\input.xml");
XmlConversionPlanConfirmation confirmation = XmlConversionPlanConfirmation.IncludeAll(preview);

service.ExportConfirmedConversionToZip(
    @"C:\path\input.xml",
    @"C:\path\output.zip",
    Encoding.UTF8,
    preview,
    confirmation);
```

Limit failures throw `XmlConversionValidationException` with structured validation issues that callers can display or log.

## Tests

The tests are in `Timeless.DataConversion.Tests` and use NUnit.

Current coverage includes:

- DataSet-based conversion.
- Streaming flat-table export.
- Structural profiling.
- Table-plan inference with structural-signature scoring.
- Conversion preview and confirmation.
- Inferred nested-table export with generated row IDs and parent row IDs.
- Zip export.
- Conversion limit validation and enforcement, including in-loop profiling/export checks.
- Public-service ambiguity rejection for low-confidence, duplicate-name, or empty-column table plans.
- XML attributes.
- CSV escaping for double quotes.
- Nested XML structures that throw schema-loading exceptions.
- Duplicate XML names that throw `DuplicateNameException`.
- Duplicate XML names with auto-renaming enabled.

The XML fixtures are stored in `tests/Timeless.DataConversion.Tests/TestData` and are copied to the test output directory during build.

Run the tests with:

```sh
dotnet test tests/Timeless.DataConversion.Tests/Timeless.DataConversion.Tests.csproj --configuration Release
```

## Notes and Limitations

- The projects target .NET 10 and use SDK-style project files.
- The `DataSet` implementation depends on how `DataSet.ReadXml` infers tables and columns from the XML shape.
- The console uses the public preview and confirmed-export workflow, not the legacy DataSet path.
- The preview-driven APIs infer a practical table plan from XML structure; XML-to-CSV conversion is not uniquely defined for every possible XML shape.
- Inferred table export writes generated relationship columns rather than preserving every XML hierarchy detail in a single flat CSV.
- The public conversion service rejects ambiguous plans; lower-level APIs keep ambiguity rejection opt-in through `XmlConversionLimits.RejectAmbiguousPlans`.
- The inferred-table exporter streams field values without materializing full row subtrees. Use `MaxRowSubtreeBytes` to bound the cumulative streamed scalar values held for a row.
- Deeply nested or ambiguous XML structures can raise framework exceptions during XML loading.
- The console application writes CSV using `Encoding.Unicode` by default. Use `-encoding utf-8` or another supported .NET encoding name to override it.
- Generated `bin` and `obj` folders are build outputs and are not needed to understand or modify the source.

## Historical Context

The change log shows the project evolved through GUI improvements, a command line front end, NUnit tests, duplicate name handling, and CSV escaping fixes. The old Windows desktop projects have been removed; the remaining solution is library, console, and tests only.
