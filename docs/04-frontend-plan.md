# Frontend Implementation Plan

## Pre-implemented Frontend Features

This document describes the frontend that will be built before the workshop.

## Tech Stack

- **Runtime**: Bun
- **Bundler**: Vite
- **Framework**: React 19 with TypeScript
- **UI Library**: shadcn/ui (Radix UI + Tailwind CSS)
- **Routing**: React Router v7
- **Server State**: TanStack Query (React Query)
- **Form Handling**: React Hook Form + Zod
- **Icons**: Lucide React
- **Date Handling**: date-fns
- **Auth**: @azure/msal-react (for admin), @azure/msal-browser
- **Testing**: Vitest + React Testing Library
- **E2E Testing**: Playwright

## Project Setup

```bash
# Initialize
bun create vite frontend --template react-ts
cd frontend
bun add react-router-dom @tanstack/react-query react-hook-form zod @hookform/resolvers date-fns lucide-react
bun add @azure/msal-react @azure/msal-browser
bun add -d tailwindcss @tailwindcss/vite vitest @testing-library/react @testing-library/jest-dom
# shadcn/ui init
bunx shadcn@latest init
```

## Pre-installed shadcn/ui Components

These will be pre-installed for attendees to use:

- `button`, `input`, `label`, `textarea`
- `card`, `badge`, `separator`
- `dialog`, `sheet`, `dropdown-menu`
- `table`, `data-table` (with pagination)
- `form` (with React Hook Form integration)
- `select`, `checkbox`, `radio-group`
- `tabs`, `accordion`
- `toast` / `sonner` (notifications)
- `skeleton` (loading states)
- `navigation-menu`, `sidebar`
- `chart` (for dashboard — Recharts based)
- `calendar`, `date-picker`

## Pre-built Pages & Components

### Public Pages

#### Event Listing (`/`)
- Grid of event cards with image, title, date, location
- Filter by upcoming/past
- Search by title
- Responsive grid layout

#### Event Detail (`/events/:id`)
- Event hero section with image, title, description
- Date, location, organizer info
- Ticket type cards with pricing
- "Buy Tickets" button → ticket selection

#### Ticket Selection (`/events/:id/tickets`)
- List of available ticket types
- Quantity selector
- Line item breakdown per ticket type (showing tax details)
- Running total calculation
- "Proceed to Checkout" button (→ 🔨 Attendee 1 implements checkout)

### Admin Pages (Protected)

#### Admin Layout
- Sidebar navigation: Events, Tax Rates, Orders, (Dashboard)
- Top bar with user info
- Breadcrumb navigation

#### Event Management (`/admin/events`)
- Data table with all events
- Status badges (Draft, Published, Cancelled)
- Actions: Edit, Publish, Archive, Delete

#### Create/Edit Event (`/admin/events/new`, `/admin/events/:id/edit`)
- Form with all event fields
- Date pickers for start/end
- Image URL input
- Max attendees
- Save as draft / Publish

#### Ticket Type Management (`/admin/events/:id/ticket-types`)
- List of ticket types for event
- Create/edit ticket type form
- **Line item editor** — dynamic form to add/remove line items
- Tax rate selector (dropdown populated from API)
- Auto-calculate total price from line items
- Available quantity management

#### Tax Rate Management (`/admin/tax-rates`)
- Data table with all tax rates
- Create/edit form
- Name, percentage, description, active toggle

#### Order List (`/admin/events/:id/orders`)
- Data table with orders for event
- Status, customer email, total, date
- Click to view order detail (skeleton)

### Shared Components

#### Layout
- `PublicLayout` — Header with navigation, footer
- `AdminLayout` — Sidebar + content area
- `Navbar` — Public navigation bar

#### Event Components
- `EventCard` — Card for event listing
- `EventForm` — Reusable form for create/edit
- `EventStatusBadge` — Color-coded status badge

#### Ticket Components
- `TicketTypeCard` — Card showing ticket with price and line items
- `TicketTypeForm` — Form with dynamic line item editor
- `TicketSelector` — Quantity picker with totals
- `LineItemEditor` — Dynamic form row for line items

#### Shared
- `DataTable` — Generic data table with sorting, pagination
- `LoadingSpinner` — Loading indicator
- `ErrorBoundary` — Error boundary with retry
- `ConfirmDialog` — Confirmation dialog
- `EmptyState` — Empty state illustration
- `CurrencyDisplay` — Formatted currency display (EUR)
- `TaxBreakdown` — Shows line items with net, tax, gross

### Hooks

- `useEvents()` — CRUD operations for events
- `useTicketTypes(eventId)` — CRUD for ticket types
- `useTaxRates()` — CRUD for tax rates
- `useOrders(eventId)` — List orders
- `useApi()` — Base fetch wrapper with error handling

### API Client

```typescript
// lib/api.ts
const API_BASE = import.meta.env.VITE_API_URL ?? '/api/v1';

// Generic fetch wrapper with:
// - Automatic JSON serialization/deserialization
// - Error handling (throws typed errors)
// - Auth token injection (for admin routes)
// - Loading/error state management via TanStack Query
```

### Auth Configuration

```typescript
// lib/auth.ts
// MSAL configuration for Entra ID (admin)
// Separate MSAL configuration for Entra External Identities (customer)
// Auth context provider
// useAuth() hook
// ProtectedRoute component for admin
```

## Styling Approach

- Tailwind CSS for utility-first styling
- shadcn/ui theming via CSS variables
- Dark mode support (prefers-color-scheme)
- Consistent spacing scale
- Custom color palette for brand

## Development Experience

```bash
# Start dev server
bun dev          # Starts Vite dev server on :5173
                 # Proxy /api/* to localhost:5000 (backend)
```

### Vite Configuration

```typescript
// vite.config.ts
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5000'
    }
  }
});
```

## Telemetry & Monitoring

The frontend uses **OpenTelemetry** for distributed tracing, mirroring the backend's approach where Application Insights is a passive collector via the OpenTelemetry pipeline.

### Packages

- `@opentelemetry/api` — Core tracing API
- `@opentelemetry/sdk-trace-web` — Web-optimized trace SDK
- `@opentelemetry/resources` — Service metadata (name, version, namespace)
- `@opentelemetry/semantic-conventions` — Standard attribute names
- `@opentelemetry/instrumentation-fetch` — Auto-instruments all `fetch()` calls, propagates `traceparent` headers to the backend API
- `@opentelemetry/instrumentation-document-load` — Records document load performance as spans
- `@opentelemetry/exporter-trace-otlp-http` — Exports traces via OTLP/HTTP to a collector or Azure Monitor
- `@opentelemetry/context-zone` — Zone.js-based async context propagation in the browser

### Architecture

```
Browser (React SPA)
  │
  ├── OpenTelemetry Web SDK
  │     ├── FetchInstrumentation   → auto-traces all API calls
  │     ├── DocumentLoadInstrumentation → page load timing
  │     └── Custom spans           → page views, form submits, user actions
  │
  └── OTLP Exporter → Application Insights (via OTLP endpoint or OTel Collector)
```

### Telemetry Service (`lib/telemetry.ts`)

```typescript
// Initialize once in main.tsx
initTelemetry();

// Track page views (call on route changes)
trackPageView('eventDetail', '/events/123');

// Custom spans for operations
const span = startSpan('checkout.submit', { 'order.itemCount': 3 });
try { /* ... */ } finally { span.end(); }

// Wrap async operations
const result = await withSpan('loadEvents', async (span) => {
  span.setAttribute('filter.status', 'published');
  return api.get<Event[]>('/events');
});
```

### Frontend Telemetry Export Strategy — Backend Proxy (Option A)

The frontend exports OTLP traces via **HTTP/JSON** (`@opentelemetry/exporter-trace-otlp-http`), not gRPC — this is the standard transport for browser-based OpenTelemetry SDKs since browsers cannot use gRPC directly.

Rather than exporting telemetry directly from the browser to Application Insights (which would require exposing a connection string or instrumentation key in client-side code), the frontend will proxy its OTLP data through the backend API:

```
Browser (OTLP/HTTP JSON) → POST /api/v1/telemetry → Backend → Azure Monitor (via managed identity)
```

**Implementation plan** (to be done when building the API endpoints):
1. Add a `POST /api/v1/telemetry` endpoint on the backend that accepts OTLP JSON trace payloads
2. The backend deserializes the incoming OTLP spans and re-exports them through its own OpenTelemetry pipeline (which already has Azure Monitor configured with managed identity)
3. The frontend's `VITE_OTLP_ENDPOINT` points to the backend origin (e.g., `/api/v1/telemetry` or via the Vite proxy in development)
4. No auth keys are exposed in the browser — the backend handles all Azure authentication

**Benefits:**
- No instrumentation key or connection string in client-side code
- Traces flow through the same Azure Monitor exporter as backend traces (managed identity)
- The backend can enrich, filter, or sample frontend spans before forwarding
- CORS is not an issue since the frontend already talks to the backend API

### Configuration

| Env Variable | Purpose | Default |
|---|---|---|
| `VITE_OTLP_ENDPOINT` | OTLP endpoint — points to the backend telemetry proxy (`/api/v1/telemetry` via Vite proxy or full backend URL) | Not set (traces created but not exported) |
| `VITE_APP_VERSION` | App version for service metadata | `0.0.0` |

### End-to-End Distributed Tracing (Frontend → Backend → Cosmos DB)

The frontend and backend share a single distributed trace for each user action:

1. **Frontend** starts a trace (e.g., user clicks "Buy Tickets")
2. **FetchInstrumentation** automatically injects a `traceparent` header (W3C Trace Context) into every `fetch()` call to the backend API
3. **Backend** (ASP.NET Core + OpenTelemetry) automatically reads the `traceparent` header and continues the same trace
4. **Cosmos DB operations** are recorded as child spans within the same trace
5. **Application Insights** correlates all spans into a single end-to-end transaction view

```
Browser                          Backend API                    Cosmos DB
  │                                │                              │
  ├─ [span: page.view]            │                              │
  ├─ [span: HTTP GET /api/v1/...] │                              │
  │   traceparent: 00-{traceId}-{spanId}-01                      │
  │ ─────────────────────────────▶ │                              │
  │                                ├─ [span: GET /api/v1/...]    │
  │                                ├─ [span: cosmos.GetItems]    │
  │                                │ ─────────────────────────▶  │
  │                                │ ◀─────────────────────────  │
  │ ◀───────────────────────────── │                              │
```

**Requirements for this to work:**
- CORS must allow the `traceparent` header — the backend's CORS config uses `AllowAnyHeader()` ✅
- `FetchInstrumentation` must include the backend URL in `propagateTraceHeaderCorsUrls` ✅
- Backend OpenTelemetry must be configured with `.WithTracing()` — ASP.NET Core auto-reads `traceparent` ✅

### What's Auto-Instrumented

- All `fetch()` calls to the backend API — with `traceparent` header propagation for end-to-end distributed traces
- Document load performance (DOM content loaded, page load timing)

### What Needs Manual Instrumentation

- Page view tracking (call `trackPageView()` on React Router route changes)
- Form submissions and user interactions (`startSpan()` or `withSpan()`)
- Error tracking in error boundaries (`trackError()`)
- Business events (e.g., "add to cart", "checkout started")

## Docker (Production)

```dockerfile
# Dockerfile
FROM oven/bun:latest AS build
WORKDIR /app
COPY package.json bun.lock ./
RUN bun install --frozen-lockfile
COPY . .
RUN bun run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

```nginx
# nginx.conf
server {
    listen 80;
    root /usr/share/nginx/html;
    index index.html;

    location /api/ {
        proxy_pass http://backend:5000;
    }

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```
