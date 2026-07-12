# XML to CSV Converter

XML to CSV Converter is a .NET Framework 4.0 solution for converting XML data into one or more CSV files. It contains a reusable conversion library, a Windows Forms desktop tool, a command line tool, and NUnit tests with sample XML files.

The main conversion path uses `DataSet.ReadXml` to discover tables in an XML document and export each discovered table to a separate CSV file. The solution also contains an alternate LINQ-to-XML based strategy.

## Solution Structure

| Path | Purpose |
| --- | --- |
| `XmlConversionLibrary` | Core conversion library. Contains the conversion strategies and shared execution context. |
| `XmlToCsvConverter` | Windows Forms application for selecting an XML file, previewing discovered tables and columns, choosing an encoding, and exporting CSV files. |
| `XmlToCsv.Console` | Command line application for converting an XML file to CSV files in an output directory. |
| `XmlToCsvTests` | NUnit 3 test project with XML fixtures for conversion behavior and known error cases. |
| `Documentation/ChangeLog.txt` | Historical change log. |

## Features

- Converts XML tables to CSV files.
- Exports each discovered XML table as a separate CSV file.
- Supports configurable output encodings in the WinForms application.
- Escapes double quotes in string values for CSV output.
- Replaces line breaks inside string values before writing CSV rows.
- Detects duplicate XML table and column naming conflicts.
- Can optionally auto-rename conflicting table names when using the `XmlToCsvUsingDataSet` strategy.
- Includes tests for basic conversion, attributes, double quote escaping, nested XML errors, and duplicate name conflicts.

## Requirements

- Windows or a compatible environment that can build .NET Framework projects.
- Visual Studio 2010 or newer with .NET Framework 4.0 targeting support.
- NUnit 3.6.0 for running the test project. The repository includes the package under `packages/NUnit.3.6.0`.

## Build

Open `XmlToCsvConverter.sln` in Visual Studio and build the solution.

From a Visual Studio Developer Command Prompt, the solution can also be built with MSBuild:

```bat
msbuild XmlToCsvConverter.sln /p:Configuration=Debug
```

## Command Line Usage

After building `XmlToCsv.Console`, run:

```bat
XmlToCsv.Console.exe -xml C:\path\input.xml -dir C:\path\output
```

Arguments:

| Argument | Description |
| --- | --- |
| `-xml` | Path to the source XML file. |
| `-dir` | Directory where generated CSV files should be written. |
| `-help` | Prints command line help. |

The console application writes one CSV file per discovered XML table using the table name as the file name.

## Windows Forms Usage

Build and run the `XmlToCsvConverter` project.

Basic workflow:

1. Select an XML file.
2. Review the discovered tables and columns.
3. Choose the desired output encoding.
4. Select a destination folder.
5. The application writes CSV files for the discovered tables.

If the XML contains a naming conflict between a table and column, the application can continue by renaming the conflicting table in the generated output.

## Library Usage

Use `XmlToCsvUsingDataSet` for the main conversion implementation:

```csharp
using System.Text;
using Moor.XmlConversionLibrary.XmlToCsvStrategy;

var converter = new XmlToCsvUsingDataSet(@"C:\path\input.xml");
var context = new XmlToCsvContext(converter);

foreach (string tableName in context.Strategy.TableNameCollection)
{
    context.Execute(tableName, @"C:\path\output\" + tableName + ".csv", Encoding.UTF8);
}
```

To allow automatic renaming when the XML has duplicate table and column names:

```csharp
var converter = new XmlToCsvUsingDataSet(@"C:\path\input.xml", true);
```

## Tests

The tests are in `XmlToCsvTests` and use NUnit 3.

Current coverage includes:

- DataSet-based conversion.
- LINQ-based conversion.
- XML attributes.
- CSV escaping for double quotes.
- Nested XML structures that throw `InvalidOperationException`.
- Duplicate XML names that throw `DuplicateNameException`.
- Duplicate XML names with auto-renaming enabled.

The XML fixtures are stored in `XmlToCsvTests/TestData` and are copied to the test output directory during build.

Run the tests with an NUnit 3-compatible runner against:

```text
XmlToCsvTests\bin\Debug\XmlToCsvTests.dll
```

## Notes and Limitations

- The project targets .NET Framework 4.0 and uses old-style Visual Studio project files.
- The `DataSet` implementation depends on how `DataSet.ReadXml` infers tables and columns from the XML shape.
- Deeply nested or ambiguous XML structures can raise framework exceptions during XML loading.
- The console application currently writes CSV using `Encoding.Unicode`.
- The WinForms application defaults `Default` and `UTF8` to UTF-8.
- Generated `bin` and `obj` folders are build outputs and are not needed to understand or modify the source.

## Historical Context

The solution file contains legacy Team Foundation Server and CodePlex source control metadata. The change log shows the project evolved through GUI improvements, a command line front end, NUnit tests, duplicate name handling, and CSV escaping fixes.
