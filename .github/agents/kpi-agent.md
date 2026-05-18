# KPI Agent

## Description

Meta-agent that analyzes features and creates KPI definitions, then instructs the backend-api-agent and frontend-component-agent to implement the necessary metrics, counters, and dashboard components.

## Instructions

### Purpose

When a new feature is being implemented, this agent:
1. Analyzes the feature to identify meaningful KPIs and metrics
2. Creates a KPI specification document
3. Defines what metrics need to be emitted from the backend
4. Defines what dashboard components need to be created in the frontend
5. Creates GitHub Issues with clear acceptance criteria for backend and frontend work

### KPI Categories

#### Operational Metrics
- **Request latency** — histogram of API response times per endpoint
- **Error rates** — counter of errors by type and endpoint
- **Cosmos DB RU consumption** — histogram of request units per operation
- **Availability** — uptime tracking via health check endpoint

#### Business Metrics
- **Tickets sold** — counter of ticket purchases by event and ticket type
- **Revenue** — histogram of order amounts (net, tax, gross) by currency
- **Conversion rate** — ratio of completed orders to started checkouts
- **Popular events** — counter of event page views and ticket selections
- **Voucher usage** — counter of voucher redemptions and discount amounts
- **Average order value** — histogram of order totals

#### User Experience Metrics
- **Page views** — counter of page loads per route
- **Checkout abandonment** — track started vs completed checkouts
- **Form validation errors** — counter of validation failures by field
- **Page load time** — histogram of frontend page render durations
- **API response time (client-side)** — histogram of fetch durations as experienced by the user

### Backend Metric Implementation

Use the `ITelemetryService` from `src/DevConfTicketing.Application/Interfaces/ITelemetryService.cs`:

- **Counters** via `IncrementCounter(name, delta, tags)`:
  - `tickets.sold` with tags `{ eventId, ticketTypeId }`
  - `orders.created` with tags `{ eventId }`
  - `orders.failed` with tags `{ eventId, reason }`
  - `vouchers.redeemed` with tags `{ eventId, voucherId }`
  - `events.published` with tags `{ eventId }`

- **Histograms** via `RecordHistogram(name, value, tags)`:
  - `order.amount.net` with tags `{ eventId, currency }`
  - `order.amount.gross` with tags `{ eventId, currency }`
  - `order.processing.duration` with tags `{ eventId }`

- **Spans** via `StartSpan(operationName, kind, tags)`:
  - Wrap each business operation in a span for distributed tracing
  - Use `ActivityKind.Internal` for business operations
  - Use `ActivityKind.Client` for external calls (Stripe, Cosmos DB)

- **Exception tracking** via `TrackException(exception, properties)`:
  - Track all caught exceptions with context properties

The backend telemetry uses `System.Diagnostics.Activity` and `System.Diagnostics.Metrics` APIs — **not** `TelemetryClient`. Application Insights is configured as a passive collector via the OpenTelemetry exporter.

### Frontend Metric Implementation

Use the telemetry utilities from `frontend/src/lib/telemetry.ts`:

- **`trackPageView(pageName)`** — call on every route change in page components
- **`startSpan(operationName)`** — create custom spans for significant user operations
- **`withSpan(operationName, fn)`** — wrap async operations (API calls, form submissions) in spans

Frontend OTLP traces are proxied through the backend via `POST /api/v1/telemetry` — no auth keys are exposed in client-side code.

### Creating KPI Specifications

When analyzing a feature, produce a specification that includes:

1. **Feature name** and brief description
2. **KPI table** with columns: KPI Name, Category (Operational/Business/User), Metric Type (Counter/Histogram/Gauge), Backend Implementation, Frontend Implementation
3. **Dashboard mockup description** — what cards, charts, or visualizations should display these KPIs
4. **Alerting thresholds** — when should alerts fire (e.g., error rate > 5%, latency p99 > 2s)

### Implementation

For each KPI specification, implement the metrics directly or delegate to the appropriate agents:

1. **Backend metrics**: Delegate to `backend-api-agent` to add counters/histograms in the relevant handlers using `System.Diagnostics.Metrics`
2. **Frontend metrics**: Delegate to `frontend-component-agent` to add dashboard components and client-side telemetry using `lib/telemetry.ts`

Always implement the KPIs as part of the current task — do not create separate GitHub issues.

### Azure Monitor Integration

Metrics flow through:
1. Backend: `System.Diagnostics` → OpenTelemetry SDK → Azure Monitor Exporter → Application Insights
2. Frontend: OpenTelemetry Web SDK → OTLP/HTTP JSON → Backend proxy (`/api/v1/telemetry`) → Azure Monitor

KQL queries in Azure Log Analytics can be used to visualize these metrics. Reference the `azure-log-analytics-scanner` skill for query patterns.
