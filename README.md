# XML to CSV Converter

Timeless.DataConversion is a .NET 10 solution for converting XML data into one or more CSV files. It contains a reusable NuGet-packable conversion library, a command line tool, and NUnit tests with sample XML files.

The conversion path uses `DataSet.ReadXml` to discover tables in an XML document and export each discovered table to a separate CSV file.

## Solution Structure

| Path | Purpose |
| --- | --- |
| `src/Timeless.DataConversion` | Core conversion library. Contains the XML-to-CSV converter. |
| `src/Timeless.DataConversion.Console` | Command line application for converting an XML file to CSV files in an output directory. |
| `tests/Timeless.DataConversion.Tests` | NUnit test project with XML fixtures for conversion behavior and known error cases. |
| `docs/ChangeLog.txt` | Historical change log. |

## Features

- Converts XML tables to CSV files.
- Exports each discovered XML table as a separate CSV file.
- Escapes double quotes in string values for CSV output.
- Replaces line breaks inside string values before writing CSV rows.
- Detects duplicate XML table and column naming conflicts.
- Can optionally auto-rename conflicting table names.
- Includes tests for basic conversion, attributes, double quote escaping, nested XML errors, and duplicate name conflicts.

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

After building `Timeless.DataConversion.Console`, run:

```bat
Timeless.DataConversion.Console.exe -xml C:\path\input.xml -dir C:\path\output -encoding utf-8
```

Arguments:

| Argument | Description |
| --- | --- |
| `-xml` | Path to the source XML file. |
| `-dir` | Directory where generated CSV files should be written. |
| `-encoding` | Optional CSV output encoding. Defaults to `unicode`. Example: `utf-8`. |
| `-help` | Prints command line help. |

The console application writes one CSV file per discovered XML table using the table name as the file name.

## Library Usage

Use `XmlToCsvUsingDataSet` for the main conversion implementation:

```csharp
using System.Text;
using Timeless.DataConversion.XmlToCsv;

using var converter = new XmlToCsvUsingDataSet(@"C:\path\input.xml");

foreach (string tableName in converter.TableNameCollection)
{
    converter.ExportToCsv(tableName, @"C:\path\output\" + tableName + ".csv", Encoding.UTF8);
}
```

To allow automatic renaming when the XML has duplicate table and column names:

```csharp
var converter = new XmlToCsvUsingDataSet(@"C:\path\input.xml", true);
```

## Tests

The tests are in `Timeless.DataConversion.Tests` and use NUnit.

Current coverage includes:

- DataSet-based conversion.
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
- Deeply nested or ambiguous XML structures can raise framework exceptions during XML loading.
- The console application writes CSV using `Encoding.Unicode` by default. Use `-encoding utf-8` or another supported .NET encoding name to override it.
- Generated `bin` and `obj` folders are build outputs and are not needed to understand or modify the source.

## Historical Context

The change log shows the project evolved through GUI improvements, a command line front end, NUnit tests, duplicate name handling, and CSV escaping fixes. The old Windows desktop projects have been removed; the remaining solution is library, console, and tests only.
