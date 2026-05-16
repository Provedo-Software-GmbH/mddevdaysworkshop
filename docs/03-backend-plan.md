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

### Order (skeleton)

```csharp
// Order.cs — Customer order
public class Order
{
    public required string Id { get; init; }
    public required string EventId { get; init; }
    public required string TicketTypeId { get; init; }
    public required int Quantity { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerId { get; set; }           // Null for guest checkout
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required List<OrderLineItem> LineItems { get; set; }
    public required decimal TotalNet { get; set; }
    public required decimal TotalTax { get; set; }
    public required decimal TotalGross { get; set; }
    public required string Currency { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
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

## 2. Infrastructure Layer (`DevConfTicketing.Infrastructure`)

### Cosmos DB Setup

- Generic `CosmosDbService` with CRUD operations
- Container configuration with partition keys
- Typed repository classes per entity
- Connection string from configuration / Key Vault
- Automatic container creation in development

### Pre-built Repositories

- `EventRepository` — Full CRUD + query by status
- `TicketTypeRepository` — CRUD scoped to eventId
- `TaxRateRepository` — CRUD + query by country
- `OrderRepository` — Create + query by eventId, skeleton only

### Monitoring

- `TelemetryService` wrapping Application Insights TelemetryClient
- Custom metrics: `EventCreated`, `OrderCreated`, `TicketTypeSold`
- Request telemetry with custom dimensions (eventId, ticketTypeId)

## 3. Application Layer (`DevConfTicketing.Application`)

Simple handler/service classes for business logic:

- `CreateEventHandler` — Validates and creates event
- `UpdateEventHandler` — Validates and updates event
- `GetEventsHandler` — Lists events with filtering
- `PublishEventHandler` — Changes status to Published
- `CreateTicketTypeHandler` — Creates ticket type with line item validation
- `CreateOrderHandler` — Creates order with tax calculation from line items
- Basic `TaxCalculationService` — Calculates totals from line item templates

## 4. API Layer (`DevConfTicketing.Api`)

### Pre-built Endpoints

All endpoints use Minimal APIs with `MapGroup()`:

```csharp
// Program.cs pattern
app.MapGroup("/api/v1/events").MapEventEndpoints();
app.MapGroup("/api/v1/events/{eventId}/ticket-types").MapTicketTypeEndpoints();
app.MapGroup("/api/v1/tax-rates").MapTaxRateEndpoints();
app.MapGroup("/api/v1/events/{eventId}/orders").MapOrderEndpoints();
```

### Middleware

- `ExceptionHandlingMiddleware` — Global error handling with ProblemDetails
- `CorrelationIdMiddleware` — Request correlation for tracing
- Health check endpoint at `/health`

### Configuration

- `appsettings.json` — Structure with placeholders
- `appsettings.Development.json` — Local development values (Cosmos emulator)
- Environment variable support for all secrets

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
