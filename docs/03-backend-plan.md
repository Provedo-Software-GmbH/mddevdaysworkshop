# Backend Implementation Plan

## Pre-implemented Backend Features

This document describes everything that will be built before the workshop.

## Solution Structure

```
src/
├── DevConfTicketing.sln
├── DevConfTicketing.Api/
├── DevConfTicketing.Domain/
├── DevConfTicketing.Application/
├── DevConfTicketing.Infrastructure/
└── DevConfTicketing.Tests/
```

## 1. Domain Layer (`DevConfTicketing.Domain`)

### Event Aggregate

```csharp
// Event.cs — Main event entity
public class Event
{
    public required string Id { get; init; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public required DateTimeOffset StartDate { get; set; }
    public required DateTimeOffset EndDate { get; set; }
    public required string OrganizerId { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public string? ImageUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public int MaxAttendees { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

// EventStatus.cs
public enum EventStatus { Draft, Published, Cancelled, Archived }
```

### Ticket Type

```csharp
// TicketType.cs — A type of ticket available for an event
public class TicketType
{
    public required string Id { get; init; }
    public required string EventId { get; init; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }      // Total price (sum of line items)
    public required string Currency { get; set; }     // "EUR"
    public int AvailableQuantity { get; set; }
    public int SoldQuantity { get; set; }
    public int MaxPerOrder { get; set; } = 10;        // 🆕 Max tickets per order (pretix-inspired)
    public bool ShowRemainingQuantity { get; set; }    // 🆕 Show "Only X left!" (pretix-inspired)
    public DateTimeOffset? SaleStart { get; set; }
    public DateTimeOffset? SaleEnd { get; set; }
    public required List<LineItemTemplate> LineItems { get; set; } // Tax-relevant breakdown
}

// LineItemTemplate.cs — Defines a line item with its tax rate
public class LineItemTemplate
{
    public required string Name { get; set; }          // e.g., "Konferenzticket", "Übernachtung"
    public required decimal NetAmount { get; set; }
    public required string TaxRateId { get; set; }     // Reference to TaxRate
    public required string TaxRateName { get; set; }   // Denormalized for display
    public required decimal TaxRatePercentage { get; set; } // Denormalized
}
```

### Tax Rate

```csharp
// TaxRate.cs — Reusable tax rate definitions
public class TaxRate
{
    public required string Id { get; init; }
    public required string CountryCode { get; init; }  // "DE"
    public required string Name { get; set; }           // "Standardsteuersatz", "Ermäßigter Satz", "Beherbergungssteuer"
    public required decimal Percentage { get; set; }    // 19.0, 7.0, 0.0
    public required string Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Order (skeleton — 🆕 upgraded with pretix-inspired features)

```csharp
// Order.cs — Customer order (supports multiple ticket types per order)
public class Order
{
    public required string Id { get; init; }
    public required string OrderCode { get; init; }   // 🆕 Human-readable code (e.g. "MDDD-A7K2")
    public required string EventId { get; init; }
    public required string CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerId { get; set; }           // Null for guest checkout
    public string? VoucherCode { get; set; }          // 🆕 Applied voucher code
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required List<OrderPosition> Positions { get; set; }  // 🆕 Multi-position orders
    public required decimal TotalNet { get; set; }
    public required decimal TotalTax { get; set; }
    public required decimal TotalGross { get; set; }
    public required decimal DiscountAmount { get; set; }  // 🆕 Voucher discount
    public required string Currency { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CancellationDate { get; set; }  // 🆕 When cancelled
}

// OrderPosition.cs — 🆕 Each position = one ticket (pretix-inspired)
public class OrderPosition
{
    public required int Index { get; init; }            // Position index within order
    public required string TicketTypeId { get; init; }
    public required string TicketTypeName { get; init; } // Denormalized
    public required string TicketSecret { get; init; }   // 🆕 Cryptographic secret for QR code
    public string? AttendeeName { get; set; }            // 🆕 Per-ticket attendee info
    public string? AttendeeEmail { get; set; }           // 🆕 Per-ticket attendee info
    public required List<OrderLineItem> LineItems { get; set; }
    public required decimal PositionNet { get; set; }
    public required decimal PositionTax { get; set; }
    public required decimal PositionGross { get; set; }
    public DateTimeOffset? CheckedInAt { get; set; }     // 🆕 Check-in timestamp
    public string? CheckedInBy { get; set; }             // 🆕 Staff user ID
}

// OrderLineItem.cs
public class OrderLineItem
{
    public required string Name { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitNetAmount { get; init; }
    public required decimal TaxRatePercentage { get; init; }
    public required string TaxRateName { get; init; }
    public decimal TotalNet => UnitNetAmount * Quantity;
    public decimal TaxAmount => TotalNet * TaxRatePercentage / 100;
    public decimal TotalGross => TotalNet + TaxAmount;
}

// OrderStatus.cs
public enum OrderStatus { Pending, PaymentProcessing, Paid, Cancelled, Refunded }
```

### Voucher (🆕 pre-built, pretix-inspired)

```csharp
// Voucher.cs — Discount voucher
public class Voucher
{
    public required string Id { get; init; }
    public required string EventId { get; init; }
    public required string Code { get; set; }            // e.g. "EARLYBIRD2026"
    public required DiscountType DiscountType { get; set; } // Percentage, Absolute, FixedPrice
    public required decimal DiscountValue { get; set; }  // e.g. 20 (for 20% or €20)
    public int MaxUsages { get; set; } = 1;
    public int UsedCount { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }
    public List<string>? ApplicableTicketTypeIds { get; set; } // null = all
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; init; }
}

// DiscountType.cs
public enum DiscountType { Percentage, Absolute, FixedPrice }
```

## 2. Infrastructure Layer (`DevConfTicketing.Infrastructure`)

### Cosmos DB Setup

- Generic `CosmosDbService` with CRUD operations
- Container configuration with partition keys
- Typed repository classes per entity
- **Authentication via Managed Identity** (no connection string) — uses `DefaultAzureCredential` + Cosmos DB account endpoint URL
- Automatic container creation in development

### Pre-built Repositories

- `EventRepository` — Full CRUD + query by status
- `TicketTypeRepository` — CRUD scoped to eventId
- `TaxRateRepository` — CRUD + query by country
- `OrderRepository` — Create + query by eventId, skeleton only
- `VoucherRepository` — 🆕 CRUD scoped to eventId + validate by code

### Monitoring

- `TelemetryService` using `System.Diagnostics.Activity` (distributed tracing) and `System.Diagnostics.Metrics` (custom metrics)
- OpenTelemetry SDK with Azure Monitor exporter (`Azure.Monitor.OpenTelemetry.AspNetCore`) — replaces the legacy Application Insights SDK
- `ActivitySource("DevConfTicketing")` for custom spans, `Meter("DevConfTicketing")` for custom metrics
- Application Insights is a passive collector via the Azure Monitor OpenTelemetry exporter
- Auto-instrumentation for ASP.NET Core requests and outgoing HTTP calls via OpenTelemetry
- **Frontend telemetry proxy** — `POST /api/v1/telemetry` endpoint accepts OTLP/HTTP JSON trace payloads from the browser and re-exports them through the backend's OpenTelemetry pipeline (Azure Monitor via managed identity). This avoids exposing any instrumentation keys in client-side code.
- **End-to-end distributed tracing**: The frontend injects `traceparent` headers (W3C Trace Context) into every API call; the backend automatically continues the same trace, creating a single correlated transaction from browser → API → Cosmos DB in Application Insights
- Service metadata enrichment (service.name, service.version, service.namespace)
- Custom metrics: `EventCreated`, `OrderCreated`, `TicketTypeSold`
- Request tracing with custom tags (eventId, ticketTypeId)

## 3. Application Layer (`DevConfTicketing.Application`)

Simple handler/service classes for business logic:

- `CreateEventHandler` — Validates and creates event
- `UpdateEventHandler` — Validates and updates event
- `GetEventsHandler` — Lists events with filtering
- `PublishEventHandler` — Changes status to Published
- `CreateTicketTypeHandler` — Creates ticket type with line item validation
- `CreateOrderHandler` — Creates order with tax calculation from line items, generates OrderCode and TicketSecrets
- Basic `TaxCalculationService` — Calculates totals from line item templates
- `VoucherValidationService` — 🆕 Validates voucher codes and calculates discounts
- `OrderCodeGenerator` — 🆕 Generates human-readable order codes (e.g. "MDDD-A7K2")
- `TicketSecretGenerator` — 🆕 Generates cryptographic secrets for QR codes

## 4. API Layer (`DevConfTicketing.Api`)

### Pre-built Endpoints

All endpoints use Minimal APIs with `MapGroup()`:

```csharp
// Program.cs pattern
app.MapGroup("/api/v1/events").MapEventEndpoints();
app.MapGroup("/api/v1/events/{eventId}/ticket-types").MapTicketTypeEndpoints();
app.MapGroup("/api/v1/tax-rates").MapTaxRateEndpoints();
app.MapGroup("/api/v1/events/{eventId}/orders").MapOrderEndpoints();
app.MapGroup("/api/v1/events/{eventId}/vouchers").MapVoucherEndpoints();  // 🆕
app.MapGroup("/api/v1/vouchers").MapVoucherValidationEndpoints();         // 🆕
```

### Middleware

- `ExceptionHandlingMiddleware` — Global error handling with ProblemDetails
- `CorrelationIdMiddleware` — Request correlation for tracing (works with incoming `traceparent` headers from the frontend for end-to-end distributed tracing)
- Health check endpoint at `/health`

### Configuration

- `appsettings.json` — Structure with placeholders
- `appsettings.Development.json` — Local development values (Cosmos emulator)
- **Azure Key Vault configuration provider** (`Azure.Extensions.AspNetCore.Configuration.Secrets`) — loads all secrets at startup via managed identity
- **No environment variables for secrets** — all sensitive config comes from Key Vault
- `DefaultAzureCredential` used for all Azure service authentication (Managed Identity in production, Azure CLI/VS in development)

```csharp
// Program.cs — Key Vault + Managed Identity setup
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

// Cosmos DB with Managed Identity
builder.Services.AddSingleton(sp =>
{
    var endpoint = builder.Configuration["CosmosDb:AccountEndpoint"];
    return new CosmosClient(endpoint, new DefaultAzureCredential());
});
```

### Swagger / OpenAPI

- OpenAPI document generation
- Scalar or SwaggerUI for API exploration

## 5. Tests (`DevConfTicketing.Tests`)

Pre-built tests to serve as examples:

- Event creation validation tests
- Tax calculation tests (7%, 19%, 0% scenarios)
- Order total calculation tests
- Integration test base class with Cosmos emulator support

## Seed Data

Development seed data loaded on startup:

```json
// Sample event
{
  "title": "MD DevDays 2026",
  "description": "Die Entwicklerkonferenz in Magdeburg",
  "location": "Magdeburg, Germany",
  "startDate": "2026-10-15",
  "endDate": "2026-10-17"
}

// Sample tax rates (German)
[
  { "name": "Standardsteuersatz", "percentage": 19.0 },
  { "name": "Ermäßigter Steuersatz", "percentage": 7.0 },
  { "name": "Beherbergungssteuer", "percentage": 0.0 },
]

// Sample ticket types
[
  {
    "name": "Konferenz-Ticket (3 Tage mit Hotel)",
    "lineItems": [
      { "name": "Konferenzticket", "netAmount": 420.17, "taxRate": "19%" },
      { "name": "Übernachtung (2 Nächte)", "netAmount": 186.92, "taxRate": "7%" },
      { "name": "Beherbergungssteuer", "netAmount": 10.00, "taxRate": "0%" },
      { "name": "Verpflegungspauschale", "netAmount": 50.42, "taxRate": "19%" }
    ]
  },
  {
    "name": "Konferenz-Ticket (Tagesticket)",
    "lineItems": [
      { "name": "Konferenzticket", "netAmount": 168.07, "taxRate": "19%" },
      { "name": "Verpflegungspauschale", "netAmount": 16.81, "taxRate": "19%" }
    ]
  }
]
```
