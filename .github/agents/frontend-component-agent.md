# Frontend Component Agent

## Description

Creates React/TypeScript components and pages following project conventions for the DevConfTicketing frontend application.

## Instructions

### Project Structure

- **Frontend root**: `frontend/`
- **Package manager**: Bun (`bun install`, `bun run dev`, `bun test`)
- **Framework**: React 19 + TypeScript + Vite 8
- **Components**: `frontend/src/components/` — reusable UI components built on shadcn/ui
- **Pages**: `frontend/src/pages/` — route-level page components
- **Hooks**: `frontend/src/hooks/` — custom React hooks for data fetching and logic
- **Types**: `frontend/src/types/` — TypeScript type definitions (one file per domain entity)
- **Layouts**: `frontend/src/layouts/` — layout components
- **Lib**: `frontend/src/lib/` — utilities, API client, auth, telemetry
- **Assets**: `frontend/src/assets/` — static assets

### Coding Conventions

- **TypeScript strictly** — never use `any`; define proper types for everything
- Use **functional components** with hooks
- Use **arrow functions** for components and handlers
- Use **named exports** (not default exports) for components
- Use **Tailwind CSS** for all styling — no inline styles, no CSS modules
- Use **shadcn/ui** components as building blocks (`button`, `card`, `badge`, etc.)
- Use **lucide-react** for icons
- Component filenames use **kebab-case** (e.g., `event-card.tsx`)
- Type filenames use **kebab-case** (e.g., `event.ts`)

### Creating New Components

1. **Create the component file** in the appropriate directory (`components/`, `pages/`)
2. **Define TypeScript types/interfaces** in `frontend/src/types/` if they don't exist
3. **Use shadcn/ui primitives** — import from `@/components/ui/`
4. **Create custom hooks** in `frontend/src/hooks/` for data fetching using TanStack Query
5. **Add proper TypeScript types** for all props, state, and API responses

### Data Fetching with TanStack Query

- Use `useQuery` for GET requests
- Use `useMutation` for POST/PUT/DELETE requests
- Create custom hooks wrapping TanStack Query calls in `frontend/src/hooks/`
- Use the API client from `frontend/src/lib/api.ts` for HTTP requests
- Define query keys as constants for cache invalidation

### API Client

- The API client is in `frontend/src/lib/api.ts`
- Vite proxy forwards `/api` requests to `http://localhost:5000`
- Use `fetch` with proper error handling
- Parse responses with Zod schemas where applicable

### Form Handling

- Use **React Hook Form** for form management
- Use **Zod** for validation schemas
- Integrate Zod schemas with React Hook Form using `@hookform/resolvers/zod`

### Styling

- Use **Tailwind CSS** utility classes exclusively
- Follow **mobile-first** responsive design (`sm:`, `md:`, `lg:` breakpoints)
- Use the project's CSS variables defined in `frontend/src/index.css`
- Use shadcn/ui's `new-york` style variant

### Accessibility

- Add **aria labels** to all interactive elements
- Ensure **keyboard navigation** works for all interactive components
- Use semantic HTML elements (`<main>`, `<nav>`, `<section>`, `<article>`)
- Provide **alt text** for images
- Ensure proper **focus management** in modals and dialogs
- Use shadcn/ui's built-in accessibility features
- Follow WCAG 2.1 AA guidelines

### Telemetry

- Import telemetry utilities from `frontend/src/lib/telemetry.ts`
- Use `trackPageView(pageName)` on route changes in page components
- Use `startSpan(operationName)` for custom spans around significant operations
- Use `withSpan(operationName, fn)` to wrap key operations (form submissions, data loads)
- Track meaningful user interactions, not every click

### Routing

- Use **React Router** for client-side routing
- Define routes in `frontend/src/App.tsx`
- Use layouts from `frontend/src/layouts/` for page structure

### Authentication

- Auth is handled via `frontend/src/lib/auth.tsx` using `@azure/msal-react`
- MSAL config is in `frontend/src/lib/msalConfig.ts`
- Environment variables: `VITE_AZURE_AD_CLIENT_ID`, `VITE_AZURE_AD_TENANT_ID`, `VITE_AZURE_AD_API_SCOPE`
- Falls back to a dev-mode mock user when env vars are not configured

### Running & Testing

- Install: `bun install`
- Dev server: `bun run dev` (runs on `http://localhost:5173`)
- Build: `bun run build`
- Lint: `bun run lint`
- Test: `bun test`
- Tests are colocated as `*.test.tsx` / `*.test.ts` files
- Use **Vitest** + **React Testing Library** for component tests
