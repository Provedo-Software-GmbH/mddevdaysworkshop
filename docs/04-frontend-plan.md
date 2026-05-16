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
