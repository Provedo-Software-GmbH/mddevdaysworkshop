# Custom Agents & Skills Plan

## Custom Agents

Custom agents are defined in `.github/agents/` as markdown files. Each agent has a name, description, instructions, and available tools.

### 1. `backend-api-agent.md`

**Purpose**: Scaffolds new .NET Minimal API endpoints following project conventions.

**Instructions include**:
- Project structure and where to place new files
- Naming conventions (PascalCase, file-scoped namespaces, records for DTOs)
- How to register endpoints in `Program.cs`
- Cosmos DB repository pattern usage
- Required using statements and patterns
- Always create request/response records in separate files
- Use primary constructors, pattern matching, nullable reference types
- Reference `.editorconfig` and `copilot-instructions.md`
- Include OpenTelemetry-based telemetry (using `System.Diagnostics.Activity` and `System.Diagnostics.Metrics`) for new operations
- Reference `TelemetryService.ActivitySourceName` and `TelemetryService.MeterName` for custom spans and metrics
- Must validate input and return proper HTTP status codes

**Tools**: Code interpreter, file operations

---

### 2. `frontend-component-agent.md`

**Purpose**: Creates React/TypeScript components and pages following project conventions.

**Instructions include**:
- Use shadcn/ui components as building blocks
- Follow the project's component structure (`components/`, `routes/`, `hooks/`)
- Use TypeScript strictly (no `any`)
- Use React Query (TanStack Query) for server state
- Create custom hooks for data fetching
- Follow existing naming patterns
- Use Tailwind CSS for styling (no inline styles, no CSS modules)
- Accessibility requirements (aria labels, keyboard navigation)
- Responsive design (mobile-first)
- Always create types in the `types/` directory
- Include OpenTelemetry tracing for new pages and significant user interactions using `lib/telemetry.ts` utilities (`startSpan`, `trackPageView`, `withSpan`)
- Track page views on route changes and wrap key operations (form submissions, data loads) in custom spans

**Tools**: Code interpreter, file operations

---

### 3. `testing-agent.md`

**Purpose**: Creates and runs unit tests for each new feature.

**Instructions include**:
- Backend: Use xUnit + FluentAssertions + NSubstitute
- Frontend: Use Vitest + React Testing Library
- Test naming: `MethodName_Scenario_ExpectedBehavior`
- Each public method/endpoint should have tests for:
  - Happy path
  - Invalid input / edge cases
  - Error handling
- Run tests after creating them and fix failures
- Place tests in `DevConfTicketing.Tests/` mirroring the source structure
- Frontend tests colocated as `*.test.tsx` / `*.test.ts`
- Aim for meaningful tests, not coverage metrics

**Tools**: Code interpreter, file operations, terminal (to run `dotnet test` and `bun test`)

---

### 4. `e2e-testing-agent.md`

**Purpose**: Creates Playwright end-to-end tests that test the full application flow.

**Instructions include**:
- Use Playwright with TypeScript
- Tests live in `frontend/e2e/tests/`
- Follow Page Object Model pattern
- Test user-facing workflows:
  - Browse events → Select tickets → Checkout → Confirmation
  - Admin: Create event → Add ticket types → View orders
- Use realistic test data
- Handle async loading states (wait for selectors, not arbitrary timeouts)
- Take screenshots on failure
- Run against the local dev server (`bun dev` + `dotnet run`)
- Must create page objects in `frontend/e2e/pages/`

**Tools**: Code interpreter, file operations, terminal (to run `bunx playwright test`)

---

### 5. `kpi-agent.md`

**Purpose**: Meta-agent that analyzes features and creates KPI definitions, then instructs backend-api-agent and frontend-component-agent to implement them.

**Instructions include**:
- Analyze the feature being implemented
- Identify meaningful KPIs and metrics:
  - Operational: Request latency, error rates, Cosmos RU consumption
  - Business: Tickets sold, revenue, conversion rate, popular events
  - User: Page views, checkout abandonment, average order value
- Create a KPI specification document
- Emit Application Insights custom metrics from the backend
- Design frontend dashboard components to visualize KPIs
- Create GitHub Issues with clear acceptance criteria for:
  - Backend: Add metric emission to relevant endpoints
  - Frontend: Add dashboard chart/card components
- Reference `ITelemetryService` patterns used in the project (Activity-based tracing, Metrics-based counters/histograms)
- For frontend features, reference `lib/telemetry.ts` and use `startSpan`, `trackPageView`, `withSpan` for custom frontend tracing

**Tools**: Code interpreter, file operations

---

## Skills

Skills are defined in `.github/copilot/skills/` as markdown files. They provide specialized knowledge to agents.

### 1. `azure-log-analytics-scanner.md`

**Purpose**: Scans Azure Log Analytics and Application Insights for errors and extracts them for agents to fix.

**Description**: This skill connects to Azure Log Analytics workspace and Application Insights to:
- Query for recent exceptions and error traces
- Extract stack traces, error messages, and affected endpoints
- Correlate errors with specific code changes (using correlation IDs)
- Format findings as actionable fix instructions

**Example usage prompt**: "Scan Application Insights for errors in the last 24 hours and create fix tasks"

**Skill content includes**:
- KQL query templates for common error patterns
- How to interpret Application Insights exception telemetry
- How to map exceptions to source code locations
- Output format with severity, frequency, affected users, and suggested fix

---

### 2. `cosmos-db-query-helper.md`

**Purpose**: Helps write efficient Cosmos DB queries and optimize RU consumption.

**Description**: This skill provides knowledge about:
- Cosmos DB SQL query syntax and best practices
- Partition key design patterns
- Cross-partition query avoidance strategies
- RU cost estimation for different query patterns
- Indexing policy optimization
- Point reads vs queries
- The project's specific container layout and partition keys

**Example usage prompt**: "Optimize this Cosmos DB query to reduce RU cost"

---

### 3. `stripe-integration-helper.md`

**Purpose**: Provides guidance on Stripe API integration patterns.

**Description**: This skill covers:
- Stripe Checkout Session creation
- Webhook handling and signature verification
- Idempotency keys for payment operations
- Error handling for Stripe API calls
- Test mode vs live mode considerations
- Common Stripe.NET SDK patterns
- PCI compliance best practices (never handle raw card data)

**Example usage prompt**: "Create a Stripe checkout session for this order"

---

### 4. `german-tax-calculation.md`

**Purpose**: Provides knowledge about German tax rules relevant to event ticketing.

**Description**: This skill covers:
- German VAT (Umsatzsteuer) rates: 19% standard, 7% reduced
- Accommodation tax (Beherbergungssteuer) — municipal tax, not subject to VAT
- Accommodation services: 7% reduced VAT rate
- Catering flat rate (Verpflegungspauschale): 19% standard VAT
- Invoice requirements under German tax law (§14 UStG):
  - Sequential invoice numbers
  - Tax ID / VAT ID
  - Itemized line items with net amount, tax rate, tax amount, gross amount
  - Separate display of different tax rates
- Reverse charge mechanism (if applicable)
- Small business regulation (Kleinunternehmerregelung) — not applicable here

**Example usage prompt**: "Calculate taxes for an order with hotel accommodation and conference tickets"

---

### 5. `accessibility-checker.md`

**Purpose**: Reviews UI components for WCAG 2.1 AA compliance.

**Description**: This skill covers:
- Color contrast requirements (4.5:1 for text, 3:1 for large text)
- Keyboard navigation patterns
- ARIA roles and attributes for common UI patterns
- Focus management in SPAs
- Screen reader compatibility
- Form validation accessibility
- Error message announcement
- shadcn/ui accessibility features and gaps to address

**Example usage prompt**: "Review this component for accessibility issues"

---

## Copilot Setup Steps

The `.github/copilot-setup-steps.yml` configures the environment for the Copilot Cloud Agent:

```yaml
steps:
  - name: Setup .NET 11
    uses: actions/setup-dotnet@v5
    with:
      dotnet-version: '11.0.x'
      dotnet-quality: 'preview'

  - name: Setup Bun
    uses: oven-sh/setup-bun@v2

  - name: Install backend dependencies
    run: dotnet restore src/DevConfTicketing.sln

  - name: Install frontend dependencies
    working-directory: frontend
    run: bun install

  - name: Install Playwright browsers
    working-directory: frontend
    run: bunx playwright install --with-deps chromium
```

## FAQ: `copilot-instructions.md`, Agents & Skills

### Do we need to update `copilot-instructions.md` to use agents and skills?

**No — not for discovery.** Agents in `.github/agents/` and skills in `.github/copilot/skills/` are automatically discovered by the Copilot cloud agent. You do not need to reference them in `copilot-instructions.md` for them to work.

**However**, it is a good practice to add a brief section to the root `copilot-instructions.md` that tells Copilot *when* to delegate to specific agents. For example:

> "For new API endpoints, delegate to `backend-api-agent`. For new React components, delegate to `frontend-component-agent`. Always run `testing-agent` after implementing a feature."

This gives the cloud agent **routing guidance** so it knows which agent to invoke for which type of task, even though it can already see all agents.

### Should we have multiple instruction files for frontend and backend?

**Yes — this is supported and recommended for this project.** According to the [official GitHub docs](https://docs.github.com/en/copilot/customizing-copilot/adding-repository-custom-instructions-for-github-copilot), GitHub Copilot supports **path-specific custom instructions** via `*.instructions.md` files in the `.github/instructions/` directory. Each file uses an `applyTo` frontmatter to specify which file paths the instructions apply to (using glob patterns).

**Important**: Path-specific instructions do **not** replace the root `copilot-instructions.md` — they are **additive**. When Copilot works on a file matching a path-specific pattern, it uses **both** the root instructions and the matching path-specific instructions.

The recommended structure is:

```
.github/copilot-instructions.md                    ← global (project-wide conventions, agent routing)
.github/instructions/backend.instructions.md       ← backend-specific (.NET, C#, API patterns)
                                                      applyTo: "src/**"
.github/instructions/frontend.instructions.md      ← frontend-specific (React, TypeScript, Tailwind)
                                                      applyTo: "frontend/**"
```

This way:

- The **root instructions** (`.github/copilot-instructions.md`) cover cross-cutting concerns: model annotations (`[Description]` + `[JsonPropertyName]`), code organization (one file per type), agent/skill routing, and shared conventions.
- **`backend.instructions.md`** (applied to `src/**`) focuses on C# language features, .NET patterns, Cosmos DB repository conventions, telemetry via `System.Diagnostics.Activity`/`Metrics`, and backend-specific conventions. This is where most of the current root C# content has been moved.
- **`frontend.instructions.md`** (applied to `frontend/**`) focuses on React 19, TypeScript (strict, no `any`), Tailwind CSS, shadcn/ui, TanStack Query, Vitest + React Testing Library, and frontend telemetry via `lib/telemetry.ts`.

**Why this matters**: The original root `copilot-instructions.md` was heavily C#-focused, which isn't helpful when the Copilot cloud agent is working on frontend files. Splitting the instructions ensures each part of the codebase gets relevant, additive guidance.

### Should we reference agents in the path-specific instructions?

**Yes, but only the relevant ones.** Each path-specific instruction file should reference the agents that apply to that area:

- `backend.instructions.md` → reference `backend-api-agent`, `testing-agent`, `kpi-agent`
- `frontend.instructions.md` → reference `frontend-component-agent`, `testing-agent`, `e2e-testing-agent`, `kpi-agent`

This keeps routing scoped — when the agent is working on files in `frontend/`, it sees frontend-relevant agents; when working on files in `src/`, it sees backend-relevant agents.

### Summary of what was implemented

1. ✅ **Slimmed down** `.github/copilot-instructions.md` to only global/cross-cutting concerns + agent routing
2. ✅ **Created** `.github/instructions/backend.instructions.md` with `applyTo: "src/**"` and backend-specific C# conventions
3. ✅ **Created** `.github/instructions/frontend.instructions.md` with `applyTo: "frontend/**"` and frontend-specific React/TypeScript conventions
4. ✅ Each path-specific instruction file references only the agents relevant to that area

---

## Agent & Skill Demonstration Plan (Workshop Intro)

During the intro session, demonstrate:

1. **Show the agent files** — explain structure and how instructions shape behavior
2. **Live demo**: Assign an issue to the Cloud Agent and watch it work
3. **Model selection**: Show how different models produce different results
4. **Skill usage**: Show how the german-tax-calculation skill guides the agent
5. **Testing agent**: Show how it automatically creates tests alongside features
6. **KPI agent**: Show the meta-agent pattern — one agent creating work for others
