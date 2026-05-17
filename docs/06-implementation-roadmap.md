# Implementation Roadmap

## Phase 1: Pre-Workshop Setup (by Workshop Leader)

This is what needs to be implemented **before** the workshop.

### Step 1: Project Scaffolding
- [ ] Create .NET 11 solution with 5 projects (Api, Domain, Application, Infrastructure, Tests)
- [ ] Create frontend with `bun create vite` + React + TypeScript
- [ ] Initialize shadcn/ui with Tailwind CSS
- [ ] Configure Vite proxy to backend
- [ ] Set up `.github/copilot-setup-steps.yml`
- [ ] Create Dockerfiles (backend + frontend)
- [ ] Set up GitHub Actions CI pipeline

### Step 2: Domain Models
- [ ] `Event`, `EventStatus`
- [ ] `TicketType`, `LineItemTemplate`
- [ ] `TaxRate`
- [ ] `Order`, `OrderLineItem`, `OrderStatus`

### Step 3: Infrastructure Layer
- [ ] Cosmos DB service (generic CRUD)
- [ ] Container configuration
- [ ] `EventRepository`
- [ ] `TicketTypeRepository`
- [ ] `TaxRateRepository`
- [ ] `OrderRepository` (basic)
- [ ] OpenTelemetry-based `TelemetryService` (Activity + Metrics)
- [ ] Telemetry in `CosmosDbService` — spans, duration histograms, operation counters for all Cosmos operations

### Step 4: Application Layer
- [ ] Event handlers (Create, Update, Get, Publish)
- [ ] TicketType handlers
- [ ] TaxRate handlers
- [ ] Basic `TaxCalculationService`
- [ ] Basic `CreateOrderHandler`
- [ ] Telemetry in all handlers — `StartSpan` for each operation, `IncrementCounter` for business events (e.g. `event.created`, `order.created`), `TrackException` on failures

### Step 5: API Layer
- [ ] Event endpoints (full CRUD)
- [ ] TicketType endpoints (full CRUD)
- [ ] TaxRate endpoints (full CRUD)
- [ ] Order endpoints (skeleton)
- [ ] Middleware (exception handling, correlation ID)
- [ ] Health check
- [ ] OpenAPI / Swagger
- [ ] CORS configuration
- [ ] Telemetry in middleware — request duration histogram, request counter with status code/endpoint tags, exception tracking in error-handling middleware

### Step 6: Frontend — Base Setup
- [ ] React Router configuration
- [ ] TanStack Query provider
- [ ] API client (`lib/api.ts`)
- [ ] Auth stub (`lib/auth.ts`)
- [ ] Public layout (header, footer, nav)
- [ ] Admin layout (sidebar, content area)
- [ ] shadcn/ui components installed

### Step 7: Frontend — Public Pages
- [ ] Event listing page (`/`)
- [ ] Event detail page (`/events/:id`)
- [ ] Ticket selection page (`/events/:id/tickets`)

### Step 8: Frontend — Admin Pages
- [ ] Event management list (`/admin/events`)
- [ ] Create event form (`/admin/events/new`)
- [ ] Edit event form (`/admin/events/:id/edit`)
- [ ] Ticket type management (`/admin/events/:id/ticket-types`)
- [ ] Tax rate management (`/admin/tax-rates`)
- [ ] Order list (`/admin/events/:id/orders`)

### Step 9: Frontend — Shared Components
- [ ] DataTable with sorting & pagination
- [ ] EventCard, EventForm, EventStatusBadge
- [ ] TicketTypeCard, TicketTypeForm, TicketSelector
- [ ] LineItemEditor (dynamic form)
- [ ] CurrencyDisplay, TaxBreakdown
- [ ] LoadingSpinner, ErrorBoundary, EmptyState, ConfirmDialog

### Step 10: Tests (Examples)
- [ ] Event creation validation tests
- [ ] Tax calculation tests (19%, 7%, 0%)
- [ ] Order total calculation tests
- [ ] Frontend: Vitest config + example component test

### Step 11: Seed Data
- [ ] Development seed data (events, tax rates, ticket types)
- [ ] Loaded on startup when in Development environment

### Step 12: Custom Agents & Skills
- [ ] `backend-api-agent.md`
- [ ] `frontend-component-agent.md`
- [ ] `testing-agent.md`
- [ ] `e2e-testing-agent.md`
- [ ] `kpi-agent.md`
- [ ] Skill: `azure-log-analytics-scanner.md`
- [ ] Skill: `cosmos-db-query-helper.md`
- [ ] Skill: `stripe-integration-helper.md`
- [ ] Skill: `german-tax-calculation.md`
- [ ] Skill: `accessibility-checker.md`

### Step 13: GitHub Issues for Attendees
- [ ] Create 6 detailed GitHub Issues (one per attendee task)
- [ ] Add labels: `workshop-task`, `attendee-1` through `attendee-6`
- [ ] Add acceptance criteria as task lists

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
