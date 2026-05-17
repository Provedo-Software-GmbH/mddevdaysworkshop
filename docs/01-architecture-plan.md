# Architecture Plan

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Azure Container Apps                   │
│                                                           │
│  ┌──────────────┐         ┌──────────────────────────┐   │
│  │   Frontend    │         │        Backend API        │   │
│  │  (nginx/SPA)  │────────▶│   .NET 11 Minimal APIs   │   │
│  │  React + TS   │         │  + Managed Identity       │   │
│  └──────────────┘         └──────────┬───────────────┘   │
│                                       │                   │
└───────────────────────────────────────┼───────────────────┘
                                        │
                    ┌───────────────────┼───────────────────┐
                    │                   │                   │
              ┌─────▼─────┐     ┌──────▼──────┐    ┌──────▼──────┐
              │  Cosmos DB │     │   Stripe    │    │  Entra ID   │
              │ (Managed ID)│    │  (Payments) │    │ (Auth)      │
              └───────────┘     └─────────────┘    └─────────────┘
                    │                                       │
              ┌─────▼─────┐                          ┌─────▼──────────┐
              │  Key Vault │                          │ Entra External │
              │ (Managed ID)│                         │  Identities    │
              └───────────┘                          └────────────────┘
                    │
              ┌─────▼──────────┐     ┌──────────────────┐
              │ Microsoft      │     │ App Insights +    │
              │ Graph API      │     │ Log Analytics     │
              │ (Email Send)   │     └──────────────────┘
              └────────────────┘
```

## Backend Architecture (.NET 11)

### Project Structure

```
src/
├── DevConfTicketing.Api/           # Minimal API Host
│   ├── Program.cs                  # Service registration, middleware
│   ├── Endpoints/                  # Endpoint definitions (grouped by feature)
│   │   ├── EventEndpoints.cs
│   │   ├── TicketTypeEndpoints.cs
│   │   ├── TaxRateEndpoints.cs
│   │   ├── OrderEndpoints.cs       # 🔨 Attendee extends
│   │   ├── PaymentEndpoints.cs     # 🔨 Attendee 1
│   │   ├── InvoiceEndpoints.cs     # 🔨 Attendee 2
│   │   ├── AuthEndpoints.cs        # 🔨 Attendee 3
│   │   ├── NotificationEndpoints.cs# 🔨 Attendee 4
│   │   ├── CheckInEndpoints.cs     # 🔨 Attendee 5
│   │   └── DashboardEndpoints.cs   # 🔨 Attendee 6
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── CorrelationIdMiddleware.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Dockerfile
│
├── DevConfTicketing.Domain/        # Domain Models (pure, no dependencies)
│   ├── Events/
│   │   ├── Event.cs
│   │   ├── EventStatus.cs
│   │   └── EventSummary.cs
│   ├── Tickets/
│   │   ├── TicketType.cs
│   │   ├── LineItemTemplate.cs     # Template for line items on a ticket type
│   │   └── TaxRate.cs
│   ├── Orders/
│   │   ├── Order.cs
│   │   ├── OrderLineItem.cs
│   │   ├── OrderStatus.cs
│   │   └── PaymentInfo.cs          # 🔨 Attendee 1
│   ├── Invoices/                   # 🔨 Attendee 2
│   │   ├── Invoice.cs
│   │   └── InvoiceLineItem.cs
│   ├── Customers/                  # 🔨 Attendee 3
│   │   ├── Customer.cs
│   │   └── CustomerType.cs
│   ├── CheckIn/                    # 🔨 Attendee 5
│   │   ├── CheckInList.cs
│   │   └── CheckInRecord.cs
│   ├── Vouchers/                   # Pre-built
│   │   └── Voucher.cs
│   └── Metrics/                    # 🔨 Attendee 6
│       └── SalesMetric.cs
│
├── DevConfTicketing.Infrastructure/ # Data access, external services
│   ├── Cosmos/
│   │   ├── CosmosDbService.cs      # Generic Cosmos operations
│   │   ├── CosmosContainerConfig.cs
│   │   └── Repositories/
│   │       ├── EventRepository.cs
│   │       ├── TicketTypeRepository.cs
│   │       ├── TaxRateRepository.cs
│   │       ├── OrderRepository.cs
│   │       ├── VoucherRepository.cs     # Pre-built
│   │       ├── CustomerRepository.cs    # 🔨 Attendee 3
│   │       ├── CheckInRepository.cs     # 🔨 Attendee 5
│   │       └── CheckInListRepository.cs # 🔨 Attendee 5
│   ├── Stripe/                     # 🔨 Attendee 1
│   │   ├── StripePaymentService.cs
│   │   └── StripeWebhookHandler.cs
│   ├── Email/                      # 🔨 Attendee 4
│   │   ├── GraphEmailService.cs    # Microsoft Graph API (Mail.Send)
│   │   └── EmailTemplates/
│   ├── Identity/                   # 🔨 Attendee 3
│   │   └── EntraExternalIdService.cs
│   └── Monitoring/
│       └── TelemetryService.cs
│
├── DevConfTicketing.Application/   # Business logic / Use cases
│   ├── Events/
│   │   ├── CreateEventHandler.cs
│   │   ├── UpdateEventHandler.cs
│   │   └── GetEventsHandler.cs
│   ├── Tickets/
│   │   ├── CreateTicketTypeHandler.cs
│   │   └── TaxCalculationService.cs  # 🔨 Attendee 2 extends
│   ├── Orders/
│   │   ├── CreateOrderHandler.cs
│   │   └── OrderService.cs
│   ├── Invoices/                   # 🔨 Attendee 2
│   │   └── InvoiceGenerationService.cs
│   ├── Payments/                   # 🔨 Attendee 1
│   │   └── PaymentService.cs
│   ├── Notifications/              # 🔨 Attendee 4
│   │   ├── NotificationService.cs
│   │   └── TicketPdfService.cs     # 🔨 Attendee 4 (PDF ticket generation)
│   ├── CheckIn/                    # 🔨 Attendee 5
│   │   └── CheckInService.cs
│   └── Dashboard/                  # 🔨 Attendee 6
│       └── MetricsService.cs
│
└── DevConfTicketing.Tests/         # Unit tests
    ├── Events/
    ├── Tickets/
    └── Orders/
```

### Key Design Decisions

1. **No EF Core with Cosmos** — Direct Cosmos DB SDK v3 usage with a generic repository pattern
2. **Minimal APIs** — Organized in endpoint classes using `MapGroup()` and extension methods
3. **Vertical Slice-adjacent** — Domain/Application/Infrastructure layers but features are cohesive
4. **Records for DTOs** — Request/Response models as records
5. **Domain models as classes** — Rich domain models with behavior
6. **End-to-end distributed tracing** — Frontend propagates `traceparent` headers (W3C Trace Context) via OpenTelemetry fetch instrumentation; backend automatically continues the trace via ASP.NET Core + OpenTelemetry; Application Insights shows a single correlated transaction from browser → API → Cosmos DB

### Cosmos DB Container Design

| Container          | Partition Key      | Description                          |
| ------------------ | ------------------ | ------------------------------------ |
| `events`           | `/id`              | Event documents                      |
| `ticket-types`     | `/eventId`         | Ticket types per event               |
| `tax-rates`        | `/countryCode`     | Tax rate definitions                 |
| `orders`           | `/eventId`         | Orders (partitioned by event)        |
| `customers`        | `/id`              | Customer profiles                    |
| `vouchers`         | `/eventId`         | Discount vouchers per event          |
| `checkin-lists`    | `/eventId`         | Check-in list definitions per event  |
| `checkin-records`  | `/eventId`         | Check-in scan records per event      |
| `invoices`         | `/orderId`         | Invoices per order                   |

### API Design

All APIs follow REST conventions with `/api/v1/` prefix:

```
# Events (pre-built)
GET    /api/v1/events
GET    /api/v1/events/{id}
POST   /api/v1/events
PUT    /api/v1/events/{id}
DELETE /api/v1/events/{id}
PATCH  /api/v1/events/{id}/publish
PATCH  /api/v1/events/{id}/archive

# Ticket Types (pre-built)
GET    /api/v1/events/{eventId}/ticket-types
POST   /api/v1/events/{eventId}/ticket-types
PUT    /api/v1/events/{eventId}/ticket-types/{id}
DELETE /api/v1/events/{eventId}/ticket-types/{id}

# Tax Rates (pre-built)
GET    /api/v1/tax-rates
POST   /api/v1/tax-rates
PUT    /api/v1/tax-rates/{id}

# Vouchers (pre-built)
GET    /api/v1/events/{eventId}/vouchers
POST   /api/v1/events/{eventId}/vouchers
PUT    /api/v1/events/{eventId}/vouchers/{id}
DELETE /api/v1/events/{eventId}/vouchers/{id}
POST   /api/v1/vouchers/validate

# Orders (pre-built skeleton, attendees extend)
POST   /api/v1/events/{eventId}/orders
GET    /api/v1/events/{eventId}/orders
GET    /api/v1/orders/{id}

# Payments — 🔨 Attendee 1
POST   /api/v1/orders/{id}/checkout
POST   /api/v1/webhooks/stripe
GET    /api/v1/orders/{id}/payment-status
POST   /api/v1/orders/{id}/cancel
POST   /api/v1/orders/{id}/refund

# Invoices — 🔨 Attendee 2
GET    /api/v1/orders/{id}/invoice
POST   /api/v1/orders/{id}/invoice/generate
GET    /api/v1/orders/{id}/invoice/pdf
POST   /api/v1/orders/{id}/invoice/cancel

# Auth / Customers — 🔨 Attendee 3
POST   /api/v1/auth/guest-checkout
POST   /api/v1/auth/register
GET    /api/v1/customers/me
GET    /api/v1/customers/me/orders

# Notifications — 🔨 Attendee 4
POST   /api/v1/orders/{id}/send-confirmation
POST   /api/v1/events/{id}/send-reminder
GET    /api/v1/notifications/templates
GET    /api/v1/orders/{id}/tickets/pdf
GET    /api/v1/orders/{id}/positions/{posId}/ticket/pdf

# Check-in & Scanning — 🔨 Attendee 5
GET    /api/v1/events/{eventId}/checkin-lists
POST   /api/v1/events/{eventId}/checkin-lists
PUT    /api/v1/events/{eventId}/checkin-lists/{id}
POST   /api/v1/events/{eventId}/checkin/redeem
GET    /api/v1/events/{eventId}/checkin/search
POST   /api/v1/events/{eventId}/checkin/{recordId}/annul
GET    /api/v1/events/{eventId}/checkin/stats
GET    /api/v1/events/{eventId}/checkin/export

# Dashboard, KPIs & Export — 🔨 Attendee 6
GET    /api/v1/dashboard/events/{eventId}/sales
GET    /api/v1/dashboard/events/{eventId}/revenue
GET    /api/v1/dashboard/overview
GET    /api/v1/events/{eventId}/export/attendees
GET    /api/v1/events/{eventId}/export/orders
GET    /api/v1/events/{eventId}/export/tax-report
```

## Frontend Architecture

### Project Structure

```
frontend/
├── index.html
├── package.json
├── bun.lock
├── vite.config.ts
├── tsconfig.json
├── tailwind.config.ts
├── components.json              # shadcn/ui config
├── Dockerfile
├── nginx.conf
├── public/
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── routes/                  # File-based or manual routing
│   │   ├── index.tsx            # Public event listing
│   │   ├── events/
│   │   │   ├── [id].tsx         # Public event detail
│   │   │   └── [id]/
│   │   │       ├── tickets.tsx  # Ticket selection → checkout
│   │   │       └── checkin.tsx # 🔨 Attendee 5 (public check-in status)
│   │   ├── checkout/
│   │   │   ├── index.tsx        # 🔨 Attendee 1 (Stripe)
│   │   │   ├── success.tsx      # 🔨 Attendee 1
│   │   │   └── cancel.tsx       # 🔨 Attendee 1
│   │   ├── account/             # 🔨 Attendee 3
│   │   │   ├── login.tsx
│   │   │   ├── register.tsx
│   │   │   └── orders.tsx
│   │   └── admin/
│   │       ├── index.tsx        # Admin overview
│   │       ├── events/
│   │       │   ├── index.tsx    # Event list (pre-built)
│   │       │   ├── new.tsx      # Create event (pre-built)
│   │       │   └── [id]/
│   │       │       ├── edit.tsx         # Edit event (pre-built)
│   │       │       ├── ticket-types.tsx # Manage ticket types (pre-built)
│   │       │       ├── orders.tsx       # View orders (pre-built skeleton)
│   │       │       ├── checkin.tsx     # 🔨 Attendee 5 (check-in lists)
│   │       │       ├── scan.tsx       # 🔨 Attendee 5 (QR scanner)
│   │       │       └── export.tsx     # 🔨 Attendee 6 (data export)
│   │       ├── tax-rates.tsx    # Tax rate management (pre-built)
│   │       ├── invoices/        # 🔨 Attendee 2
│   │       ├── customers/       # 🔨 Attendee 3
│   │       ├── notifications/   # 🔨 Attendee 4
│   │       └── dashboard/       # 🔨 Attendee 6
│   ├── components/
│   │   ├── ui/                  # shadcn/ui components
│   │   ├── layout/
│   │   │   ├── PublicLayout.tsx
│   │   │   ├── AdminLayout.tsx
│   │   │   └── Navbar.tsx
│   │   ├── events/
│   │   │   ├── EventCard.tsx
│   │   │   ├── EventForm.tsx
│   │   │   └── EventDetail.tsx
│   │   ├── tickets/
│   │   │   ├── TicketTypeCard.tsx
│   │   │   ├── TicketTypeForm.tsx
│   │   │   └── TicketSelector.tsx
│   │   └── shared/
│   │       ├── DataTable.tsx
│   │       ├── LoadingSpinner.tsx
│   │       └── ErrorBoundary.tsx
│   ├── hooks/
│   │   ├── useEvents.ts
│   │   ├── useTicketTypes.ts
│   │   └── useApi.ts
│   ├── lib/
│   │   ├── api.ts               # API client (fetch wrapper)
│   │   ├── auth.ts              # Auth utilities
│   │   ├── telemetry.ts         # OpenTelemetry tracing (distributed traces, fetch instrumentation, custom spans)
│   │   └── utils.ts             # shadcn/ui utilities
│   └── types/
│       ├── event.ts
│       ├── ticket.ts
│       ├── order.ts
│       └── api.ts
└── e2e/                         # 🔨 Playwright tests (e2e-testing-agent)
    ├── playwright.config.ts
    └── tests/
```

## Authentication Architecture

### Admin Users (Internal)

- **Microsoft Entra ID** with an App Registration
- Backend validates JWT tokens from Entra ID
- Admin endpoints protected via `[Authorize]` with role-based policies
- Frontend uses MSAL.js for login flow

### Customers (External)

- **Microsoft Entra External Identities** (CIAM)
- Guest checkout (no account required) — creates order with email only
- Optional registration for order history
- Frontend uses MSAL.js with external tenant configuration

## Azure Infrastructure (Production)

| Service                        | SKU / Tier              | Purpose                        |
| ------------------------------ | ----------------------- | ------------------------------ |
| Azure Container Apps           | Consumption (serverless)| Backend API + Frontend SPA     |
| Azure Cosmos DB                | Serverless              | Database (Managed Identity)    |
| Azure Container Registry       | **Existing** (reuse)    | Docker images                  |
| Azure Application Insights     | Pay-as-you-go           | APM, logging, traces (via OpenTelemetry — Backend & Frontend) |
| Azure Log Analytics Workspace  | Pay-as-you-go           | Centralized logs               |
| Azure Key Vault                | Standard                | Secrets (ASP.NET config provider) |
| User-Assigned Managed Identity | Free                    | Auth to Azure services         |
| Microsoft Graph API            | Included (M365)         | Email sending (Mail.Send)      |
| Entra ID                       | Included (Free tier)    | Admin auth                     |
| Entra External Identities      | Pay-as-you-go (MAU)     | Customer auth                  |

Estimated monthly cost (low traffic): **~€7-20/month**

> **Key design decisions**:
> - **Managed Identities** for all Azure service access (no connection strings)
> - **ASP.NET Key Vault configuration provider** for secrets (not Container App env vars)
> - **Microsoft Graph API** for email (not Azure Communication Services)
> - **Existing ACR** is reused (no new registry provisioned)

## CI/CD Pipeline

```yaml
# .github/workflows/ci.yml
- Trigger: PR and push to main
- Steps:
  1. Build & test backend (.NET 11)
  2. Build & lint frontend (Bun)
  3. Run unit tests
  4. Run Playwright e2e tests (on PR only if label present)

# .github/workflows/deploy.yml (future, not part of workshop)
- Trigger: Push to main
- Steps:
  1. Build Docker images
  2. Push to ACR
  3. Deploy to Azure Container Apps
```
