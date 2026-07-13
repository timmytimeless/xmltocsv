# Public XML to CSV Conversion Specification

This document describes the target architecture for a public-facing XML to CSV conversion solution that must accept arbitrary XML files without assuming an XSD is present.

The system must support schema inference, large files, nested XML, and user-facing conversion previews without loading entire XML documents into memory.

## Core Constraint

There is no perfect solution that is simultaneously:

- fully general,
- schema-inferring,
- memory-safe for 1 GB files,
- and guaranteed to produce intuitive CSV.

XML is hierarchical; CSV is tabular. Without an XSD or user-provided mapping, the schema is not uniquely defined. The system must therefore infer a conversion plan, show that plan to the user, and allow confirmation or adjustment before final export.

## Target Flow

`Upload XML -> streaming structural inference -> inferred table plan -> preview -> streaming export -> zip of CSVs`

## Required Steps

### 1. Streaming Profiling Pass

Read the full XML with `XmlReader` and build a structural profile:

- element paths
- attribute paths
- repeated elements
- occurrence counts
- max depth
- candidate row/table nodes
- candidate columns per row path
- type hints per path

This must be memory-safe by storing paths and statistics, not the full document.

### 2. Infer Candidate Tables

Based on the structural profile, infer likely table roots:

- repeated sibling elements
- repeated paths with similar child/attribute structure
- leaf-heavy structures
- high row counts

Example: `/orders/order` becomes a table; `/orders/order/items/item` may become another table.

### 3. Generate Multiple CSVs

For nested or repeated structures, emit multiple related CSVs instead of flattening everything into one file:

- `order.csv`
- `order_item.csv`
- generated parent IDs / row IDs

This is the preferred way to support arbitrary nested XML without losing structure.

### 4. Preview and User Confirmation

For a website, show inferred tables and columns before conversion:

- detected tables
- row counts
- columns
- nested child tables
- warnings for ambiguous structures

Users should be able to include/exclude tables and rename tables or columns before export.

### 5. Second Streaming Export Pass

After inference and confirmation, stream the XML again and write CSVs according to the confirmed conversion plan.

The exporter must not load the full XML document into memory.

### 6. Hard Operational Limits

A public service must enforce operational limits:

- max file size
- max XML depth
- max unique paths
- max columns per table
- max generated CSV files
- timeout/cancellation
- output zip size limits

These limits must be surfaced as user-facing validation or conversion errors.

### 7. Converter-Level Limit Enforcement

Reusable validators are useful for clients and GUIs, but public conversion workflows must also provide converter-level APIs that enforce configured limits.

The converter should expose validation-aware workflow methods that accept limit settings and stop conversion when those limits are exceeded. These methods should apply limits at the appropriate stages:

- before profiling: file size
- during or immediately after profiling: XML depth and unique paths
- after table-plan inference or confirmation: max columns per table and max generated CSV files
- during export or immediately after export: timeout/cancellation and output size limits

For example, a configured max XML depth must be enforceable by calling a converter workflow method, not only by requiring client code to remember to call a separate validator.

Validation failures should be returned or thrown as structured conversion errors that clients can display directly.

### 8. Reject or Require Mapping for Pathological XML

Some XML cannot be converted meaningfully without user decisions. The service should return an actionable message instead of silently producing bad CSV.

When the inferred structure is ambiguous or too complex, the system must either:

- ask the user to adjust the inferred conversion plan,
- require an explicit mapping,
- or reject the conversion with a clear explanation.

## Implementation Direction

The current `DataSet`-based approach is useful for simple or medium-sized files, but it is not suitable as the core architecture for a public 1 GB arbitrary XML conversion service.

The long-term core should be a streaming XML profiler plus table-plan generator, followed by a streaming exporter.
