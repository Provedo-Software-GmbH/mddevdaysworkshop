---
applyTo: "frontend/**"
---

# Frontend Instructions (React / TypeScript)

## Agent Routing

- For new React components or pages, delegate to `frontend-component-agent`
- For unit tests, delegate to `testing-agent`
- For end-to-end tests, delegate to `e2e-testing-agent`
- For KPI/metrics work, delegate to `kpi-agent`

## TypeScript

- Use TypeScript strictly — no `any` types
- Use interfaces for object shapes, type aliases for unions/intersections
- Always create types in the `types/` directory
- Use `satisfies` operator for type-safe object literals
- Prefer `const` assertions for literal types

## React

- Use React 19 with functional components only (no class components)
- Use React Query (TanStack Query) for all server state management
- Create custom hooks for data fetching in `hooks/`
- Use `Suspense` and error boundaries for async loading states
- Follow existing naming patterns for components and hooks

## UI & Styling

- Use shadcn/ui components as building blocks
- Use Tailwind CSS for styling (no inline styles, no CSS modules)
- Mobile-first responsive design
- Follow the project's component structure (`components/`, `routes/`, `hooks/`)

## Accessibility

- WCAG 2.1 AA compliance required
- Use aria labels and keyboard navigation
- Color contrast: 4.5:1 for text, 3:1 for large text
- Focus management in SPAs
- Form validation accessibility

## Frontend Telemetry

- Use OpenTelemetry Web SDK via `lib/telemetry.ts`
- Use `startSpan()`, `trackPageView()`, `withSpan()` for custom spans
- Track page views on route changes
- Wrap key operations (form submissions, data loads) in custom spans
- OTLP traces are proxied through the backend via `POST /api/v1/telemetry`

## Frontend Testing

- Unit tests: Vitest + React Testing Library
- E2E tests: Playwright with Page Object Model pattern
- Test files colocated as `*.test.tsx` / `*.test.ts`
- E2E tests in `frontend/e2e/tests/`, page objects in `frontend/e2e/pages/`
- Run unit tests with: `cd frontend && bun test`
- Run E2E with: `cd frontend && bunx playwright test`

## Package Manager

- Use Bun as the package manager and runtime
- Install dependencies with: `cd frontend && bun install`
- Dev server: `cd frontend && bun dev`
