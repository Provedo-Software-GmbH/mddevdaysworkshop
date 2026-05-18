# Backend API Agent

## Description

Scaffolds new .NET Minimal API endpoints following project conventions for the DevConfTicketing application.

## Instructions

### Project Structure

- **Solution**: `src/DevConfTicketing.slnx` (modern `.slnx` format)
- **API layer**: `src/DevConfTicketing.Api/` — Minimal API endpoints in `Endpoints/` directory
- **Application layer**: `src/DevConfTicketing.Application/` — Business logic handlers
- **Domain layer**: `src/DevConfTicketing.Domain/` — Domain models (records with `[Description]` and `[JsonPropertyName]` attributes)
- **Infrastructure layer**: `src/DevConfTicketing.Infrastructure/` — Cosmos DB repositories, telemetry
- **Tests**: `src/DevConfTicketing.Tests/` — xUnit tests mirroring source structure
- **Target framework**: .NET 11 (`net11.0`) with preview language features

### Coding Conventions

- Use **file-scoped namespaces** for all files
- Use **primary constructors** for classes and structs
- Use **records** for DTOs and request/response types
- Use **PascalCase** for types, methods, properties, and events
- Use **camelCase** for local variables and parameters
- Use **_camelCase** (underscore prefix) for private fields
- Use **pattern matching** (switch expressions, `is` patterns, property patterns)
- Use **nullable reference types** — always check for nullability
- Use **target-typed `new()`** when the type is clear from context
- Use **collection expressions** (`[]` syntax) instead of traditional initializers
- Use **`nameof`** instead of hardcoded strings
- Refer to `.editorconfig` and `.github/copilot-instructions.md` for full style guide

### Creating New Endpoints

1. **Create the endpoint file** in `src/DevConfTicketing.Api/Endpoints/` following the `{Entity}Endpoints.cs` naming pattern
2. **Define a static class** with a `Map{Entity}Endpoints` extension method on `RouteGroupBuilder`
3. **Create request/response records** in the same file, annotated with `[Description]` and `[JsonPropertyName]` attributes on every property
4. **Register the route group** in `Program.cs` using `app.MapGroup("/api/v1/{route}").Map{Entity}Endpoints()`
5. **Use proper HTTP methods**: GET for reads, POST for creates, PUT for updates, DELETE for deletes
6. **Return proper status codes**: `Results.Ok()`, `Results.Created()`, `Results.NoContent()`, `Results.NotFound()`, `Results.Conflict()`, `Results.ValidationProblem()`
7. **Add metadata**: `.WithName()`, `.WithTags()`, `.WithDescription()`, `.Produces<T>()`, `.ProducesProblem()`
8. **Add authorization** where needed: `.RequireAuthorization("AdminPolicy")` or `.RequireAuthorization("EventManagerPolicy")`

### Example Endpoint Pattern

Follow the pattern established in `src/DevConfTicketing.Api/Endpoints/EventEndpoints.cs`:

```csharp
public static class {Entity}Endpoints
{
    public static RouteGroupBuilder Map{Entity}Endpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async ({Handler} handler, CancellationToken ct) => { ... })
            .WithName("Get{Entities}")
            .WithTags("{Entities}")
            .WithDescription("...")
            .Produces<IReadOnlyList<{Entity}>>();

        return group;
    }
}
```

### Application Layer Handlers

- Create handler classes in `src/DevConfTicketing.Application/{Feature}/`
- Use **primary constructors** to inject dependencies (repositories, telemetry service)
- Register handlers as scoped services in `Program.cs`: `builder.Services.AddScoped<{Handler}>()`
- Handlers should contain business logic, validation, and orchestration

### Repository Pattern

- Repositories are defined as interfaces in `src/DevConfTicketing.Application/Interfaces/`
- Implementations live in `src/DevConfTicketing.Infrastructure/Cosmos/Repositories/`
- Use the `CosmosDbService` for all Cosmos DB operations
- Each repository has a `CosmosContainerConfig` defining container name and partition key path
- Repositories are registered as singletons in `InfrastructureServiceRegistration`

### Telemetry

- Inject `ITelemetryService` into handlers
- Use `StartSpan(operationName)` to create spans for each operation — wrap in `using` blocks
- Use `IncrementCounter(metricName)` for business events (e.g., `event.created`, `order.created`)
- Use `TrackException(exception)` in catch blocks
- Reference `TelemetryService.ActivitySourceName` (`"DevConfTicketing"`) and `TelemetryService.MeterName` (`"DevConfTicketing"`) for custom spans and metrics
- Telemetry uses `System.Diagnostics.Activity` and `System.Diagnostics.Metrics` APIs — **not** `TelemetryClient`

### Model Annotations

All domain models and DTOs **must** have:
- `[Description("...")]` attribute on the type and all properties
- `[JsonPropertyName("...")]` attribute on all properties (camelCase)

### Input Validation

- Validate all input in handlers before processing
- Return `Results.ValidationProblem()` for invalid input
- Check for null/empty required fields
- Validate date ranges, numeric constraints, and business rules

### Authentication & Authorization

- Uses Microsoft Entra ID via `Microsoft.Identity.Web` (JWT Bearer)
- **AdminPolicy**: requires `Admin` or `EventManager` role
- **EventManagerPolicy**: requires `EventManager` or `Admin` role
- Public endpoints (e.g., browsing events) do not require authorization

### Using Statements

Common using statements for endpoint files:
```csharp
using System.ComponentModel;
using System.Text.Json.Serialization;
```

### Running & Testing

- Build: `dotnet build src/DevConfTicketing.slnx`
- Test: `dotnet test src/DevConfTicketing.slnx`
- The API runs on `http://localhost:5000`
