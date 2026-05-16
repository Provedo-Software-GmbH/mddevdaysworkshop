# Copilot Instructions

## C# Language Features

We always use the latest C# language features:

### Pattern Matching
- Use pattern matching where possible (switch expressions, `is` patterns, property patterns)
- Use list patterns for collection matching
- Use relational patterns (`>`, `<`, `>=`, `<=`) and logical patterns (`and`, `or`, `not`)

### Type System & Nullability
- Enable nullable reference types and check for nullability where possible
- Use `required` members to enforce initialization at compile time
- Use init-only setters (`init`) for immutable properties

### Modern Syntax
- Use target-typed `new()` expressions when the type is clear from context
- Use collection expressions (`[]` syntax) instead of traditional initializers
- Use raw string literals (`"""`) for multi-line strings and strings with special characters
- Use UTF-8 string literals (`"text"u8`) for performance-critical scenarios
- Use file-scoped namespaces to reduce indentation
- Use global usings for commonly used namespaces
- Use the `nameof` operator instead of hardcoded strings

### Records & Immutability
- Use records for immutable data types with value-based equality
- Use record structs for value-type records
- Use `with` expressions to create modified copies of records

### Classes & Methods
- Use primary constructors for classes and structs where appropriate
- Use the new extensions instead of regular extension methods
- Use static abstract/virtual members in interfaces where applicable
- Use default lambda parameters when appropriate

### Async & Streaming
- Use `IAsyncEnumerable<T>` and `await foreach` for async streams
- Use `ValueTask` and `ValueTask<T>` for performance-critical async operations

### Performance & Memory
- Use `Span<T>` and `ReadOnlySpan<T>` for stack-allocated memory and slicing operations
- Use `Memory<T>` and `ReadOnlyMemory<T>` for heap-allocated memory that needs to be passed around
- Use ranges (`..`) and indices (`^`) for array/span slicing
- Use `stackalloc` with `Span<T>` for small, short-lived allocations
- Use `params` with collection types (not just arrays) for flexible parameter lists
- Prefer `StringComparison` overloads for string operations

## Code Organization

- **One file per type**: Always create one file per class, record, struct, enum, or interface
- **File-scoped namespaces**: Use file-scoped namespace declarations

## Coding Standards

Refer to the `.editorconfig` file in the repository root for all coding and naming guidelines. The `.editorconfig` is comprehensive and well-maintained.

Key conventions from `.editorconfig`:
- Use PascalCase for types, methods, properties, and events
- Use camelCase for local variables, parameters, and local constants
- Use _camelCase (underscore prefix) for private fields
- Use IPascalCase (I prefix) for interfaces
- Prefer expression-bodied members for accessors, indexers, lambdas, and properties
- Use file-scoped namespace declarations
- Use primary constructors where appropriate

## Dependencies and Framework

- Stay on the latest version of .NET, including preview versions
- Stay on the latest version of libraries, including preview versions
- Prefer modern, actively maintained packages
