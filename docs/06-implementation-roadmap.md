# Implementation Roadmap

## Phase 1: Pre-Workshop Setup (by Workshop Leader)

This is what needs to be implemented **before** the workshop.

### Step 1: Project Scaffolding ✅

- [x] Create .NET 11 solution with 5 projects (Api, Domain, Application, Infrastructure, Tests) — using modern `.slnx` format, `Directory.Build.props` targets `net11.0` with preview language features, nullable reference types enabled
- [x] Create frontend with `bun create vite` + React 19 + TypeScript — Vite 8, Bun package manager, React Router, TanStack Query, Zod, React Hook Form installed
- [x] Initialize shadcn/ui with Tailwind CSS — `new-york` style, CSS variables, `lucide-react` icons, `button`, `card`, `badge` components installed
- [x] Configure Vite proxy to backend — `/api` → `http://localhost:5000`
- [x] Set up `.github/workflows/copilot-setup-steps.yml` — installs .NET 11 preview + Bun, restores dependencies
- [x] Create Dockerfiles (backend + frontend) — backend: multi-stage SDK → ASP.NET runtime on port 5000; frontend: Bun build → nginx:alpine with SPA routing + API proxy
- [x] Set up GitHub Actions CI pipeline — `ci.yml` with concurrent backend (.NET build/test/lint) and frontend (Bun install/lint/build) jobs

### Step 2: Domain Models ✅

All models annotated with `[Description]` and `[JsonPropertyName]` attributes.

- [x] `Event`, `EventStatus` — Event with title, description, location, dates, organizer, capacity, image/website URLs, timestamps; EventStatus enum (Draft, Published, Cancelled, Archived)
- [x] `TicketType`, `LineItemTemplate` — TicketType with price, currency, quantity tracking, sale window, max per order; LineItemTemplate with net amount and tax rate reference
- [x] `TaxRate` — country code (partition key), percentage, name, description, default/active flags
- [x] `Order`, `OrderLineItem`, `OrderPosition`, `OrderStatus` — Order with customer info, voucher support, financial breakdown (net/tax/gross/discount), positions; OrderLineItem with computed totals; OrderPosition with attendee info, ticket secret, check-in tracking; OrderStatus enum
- [x] `Voucher`, `DiscountType` — Voucher with discount type/value, usage limits, validity window, applicable ticket type filtering

### Step 3: Infrastructure Layer ✅

- [x] Cosmos DB service (generic CRUD) — `CosmosDbService` with GetItem, GetItems, QueryItems, Create, Upsert, Delete; camelCase serialization; development emulator key / production `DefaultAzureCredential`
- [x] Container configuration — `CosmosContainerConfig` with container name and partition key path
- [x] `EventRepository` — container `"events"`, partition key `/id`
- [x] `TicketTypeRepository` — container `"ticket-types"`, partition key `/eventId`
- [x] `TaxRateRepository` — container `"tax-rates"`, partition key `/countryCode`
- [x] `OrderRepository` — container `"orders"`, partition key `/eventId`
- [x] `VoucherRepository` — container `"vouchers"`, partition key `/eventId`
- [x] OpenTelemetry-based `TelemetryService` (Activity + Metrics) — ActivitySource `"DevConfTicketing"`, Meter `"DevConfTicketing"`, `StartSpan`, `TrackEvent`, `TrackException`, `RecordHistogram`, `IncrementCounter` with `ConcurrentDictionary` caching
- [x] Telemetry in `CosmosDbService` — spans with `db.system=cosmosdb` tags, `db.cosmos.duration` histograms, `db.cosmos.operations` counters (success/error), exception tracking
- [x] Infrastructure service registration — `InfrastructureServiceRegistration` with Cosmos DB setup, repository singletons, OpenTelemetry tracing/metrics configuration, optional Azure Monitor exporter, database initialization (`InitializeCosmosDbAsync`)

### Step 4: Application Layer ✅
- [x] Event handlers (Create, Update, Get, Publish)
- [x] TicketType handlers
- [x] TaxRate handlers
- [x] Basic `TaxCalculationService`
- [x] Basic `CreateOrderHandler`
- [x] Telemetry in all handlers — `StartSpan` for each operation, `IncrementCounter` for business events (e.g. `event.created`, `order.created`), `TrackException` on failures

### Step 5: API Layer ✅
- [x] Event endpoints (full CRUD)
- [x] TicketType endpoints (full CRUD)
- [x] TaxRate endpoints (full CRUD)
- [x] Order endpoints (skeleton)
- [x] Middleware (exception handling, correlation ID)
- [x] Health check
- [x] OpenAPI / Swagger
- [x] CORS configuration (must allow `traceparent` header for end-to-end distributed tracing with the frontend)
- [x] Frontend telemetry proxy endpoint (`POST /api/v1/telemetry`) — accepts OTLP/HTTP JSON traces from the browser and re-exports via the backend's OpenTelemetry pipeline (no auth keys in the browser)
- [x] Telemetry in middleware — request duration histogram, request counter with status code/endpoint tags, exception tracking in error-handling middleware
- [x] Admin authentication (Entra ID) — `Microsoft.Identity.Web` JWT Bearer validation, `AdminPolicy` and `EventManagerPolicy` authorization policies, admin write endpoints protected with `RequireAuthorization("AdminPolicy")`, GET endpoints remain public

### Step 6: Frontend — Base Setup ✅
- [x] React Router configuration
- [x] TanStack Query provider
- [x] API client (`lib/api.ts`) — generic fetch wrapper with auth-token injection for admin routes
- [x] Admin authentication (`lib/auth.tsx`, `lib/msalConfig.ts`) — `@azure/msal-react` + `@azure/msal-browser` for Entra ID login, `AuthProvider` wraps `MsalProvider` when configured (`VITE_AZURE_AD_CLIENT_ID`), falls back to dev-mode mock user; `useAuth()` hook, `ProtectedRoute` component, automatic Bearer token injection via API client
- [x] OpenTelemetry telemetry service (`lib/telemetry.ts`) — distributed tracing, fetch auto-instrumentation with `traceparent` header propagation (W3C Trace Context), document load spans, custom span helpers. Uses OTLP/HTTP (not gRPC) — browser-compatible. Traces exported via backend proxy (`POST /api/v1/telemetry`) to avoid exposing auth keys in the browser.
- [x] Telemetry initialization in `main.tsx`
- [x] End-to-end distributed tracing wired: frontend `traceparent` → backend continues trace → Cosmos DB spans in same trace (runtime verification requires deployed infrastructure)
- [x] Public layout (header, footer, nav)
- [x] Admin layout (sidebar, content area) with `useAuth()` integration
- [x] shadcn/ui components installed

### Step 7: Frontend — Public Pages ✅
- [x] Event listing page (`/`) — responsive grid of event cards with search by title, filter by upcoming/past (tabs), loading skeletons, empty states
- [x] Event detail page (`/events/:id`) — hero section with image/gradient placeholder, info grid (dates, location, capacity), ticket type cards with pricing, "Buy Tickets" CTA
- [x] Ticket selection page (`/events/:id/tickets`) — quantity selectors per ticket type (respects maxPerOrder & availability), line item breakdown with net/tax/gross, sticky order summary with grand total, "Proceed to Checkout" placeholder
- [x] Page view telemetry on route changes (`trackPageView`) — tracked on all public pages via `useEffect` + `useLocation`

### Step 8: Frontend — Admin Pages ✅
- [x] Event management list (`/admin/events`)
- [x] Create event form (`/admin/events/new`)
- [x] Edit event form (`/admin/events/:id/edit`)
- [x] Ticket type management (`/admin/events/:id/ticket-types`)
- [x] Tax rate management (`/admin/tax-rates`)
- [x] Order list (`/admin/events/:id/orders`)
- [x] Telemetry spans for form submissions and admin actions

### Step 9: Frontend — Shared Components ✅
- [x] DataTable with sorting & pagination
- [x] EventCard, EventForm, EventStatusBadge
- [x] TicketTypeCard, TicketTypeForm, TicketSelector
- [x] LineItemEditor (dynamic form)
- [x] CurrencyDisplay, TaxBreakdown
- [x] LoadingSpinner, ErrorBoundary (with `trackError` telemetry), EmptyState, ConfirmDialog

### Step 10: Tests (Examples) ✅
- [x] Event creation validation tests
- [x] Tax calculation tests (19%, 7%, 0%)
- [x] Order total calculation tests
- [x] Frontend: Vitest config + example component test

### Step 11: Seed Data ✅
- [x] Development seed data (events, tax rates, ticket types)
- [x] Loaded on startup when in Development environment

### Step 12: Custom Agents & Skills ✅
- [x] `backend-api-agent.md`
- [x] `frontend-component-agent.md`
- [x] `testing-agent.md`
- [x] `e2e-testing-agent.md`
- [x] `kpi-agent.md`
- [x] Skill: `azure-log-analytics-scanner.md`
- [x] Skill: `cosmos-db-query-helper.md`
- [x] Skill: `stripe-integration-helper.md`
- [x] Skill: `german-tax-calculation.md`
- [x] Skill: `accessibility-checker.md`

### Step 13: GitHub Issues for Attendees ✅
- [x] Create 6 detailed GitHub Issues (one per attendee task)
- [ ] Add labels: `workshop-task`, `attendee-1` through `attendee-6` *(requires manual label creation — token lacked permission)*
- [x] Add acceptance criteria as task lists

### Step 14: Documentation
- [ ] README.md update with setup instructions
- [ ] CONTRIBUTING.md with development guide
- [ ] Workshop guide for attendees

---

## Phase 2: Workshop Day

### Pre-Workshop Prep
- [ ] Verify all attendees have GitHub Copilot access
- [ ] Add Stripe test API keys to GitHub Secrets
- [ ] Verify Cosmos DB is accessible
- [ ] All attendees fork the repo to their org

### During Workshop
1. Intro (1h): Architecture walkthrough, agent demo
2. Setup (30min): Fork, activate Copilot Agent, first test issue
3. Hands-on (4h): Each attendee works on assigned task
4. Review (30min): PR reviews, demos, lessons learned

---

## Implementation Order Recommendation

For the pre-workshop implementation, I recommend this order:

1. **Project scaffolding** (both backend and frontend) — this unblocks everything
2. **Domain models** — foundation for all features
3. **Cosmos DB infrastructure** — needed for any data persistence
4. **Backend API endpoints** (Events, TicketTypes, TaxRates)
5. **Frontend base setup** (routing, layouts, API client)
6. **Frontend pages** (event listing, admin CRUD)
7. **Seed data** — makes the app feel alive immediately
8. **Tests** — example tests that attendees can follow
9. **Custom agents & skills** — the workshop's differentiator
10. **CI pipeline** — ensures everything stays green
11. **Dockerfiles** — last, as they just package what's built
12. **GitHub Issues** — final step, references the code that exists
