# DevConf Ticketing — Workshop Repository

> KI-gestützte Entwicklung mit GitHub Copilot Agents

A full-day hands-on workshop where 6 experienced developers learn agent-based development using a **real Event Management & Ticketing System** built with .NET 11 and React 19.

## Tech Stack

| Layer | Technology |
| ----- | ---------- |
| Backend | .NET 11 Minimal APIs, Cosmos DB SDK |
| Frontend | React 19, TypeScript, Vite 8, shadcn/ui, Bun |
| Auth (Admin) | Microsoft Entra ID |
| Auth (Customer) | Microsoft Entra External Identities |
| Payments | Stripe (Test Mode) |
| Monitoring | OpenTelemetry SDK → Azure Monitor / Application Insights |
| Hosting | Azure Container Apps |
| CI/CD | GitHub Actions |

## Prerequisites

- [.NET 11 SDK (Preview)](https://dotnet.microsoft.com/download/dotnet/11.0)
- [Bun](https://bun.sh/) (latest)
- [Azure Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator) or a Cosmos DB account
- [Git](https://git-scm.com/)
- [Docker](https://www.docker.com/) (optional, for containerized runs)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/sia-consulting/mddevdaysworkshop.git
cd mddevdaysworkshop
```

### 2. Backend Setup

```bash
# Restore dependencies
dotnet restore src/DevConfTicketing.slnx

# Start the Cosmos DB Emulator (or use an Azure Cosmos DB instance)
# The default connection is configured in src/DevConfTicketing.Api/appsettings.Development.json

# Run the API
dotnet run --project src/DevConfTicketing.Api
```

The API will start on `http://localhost:5000` with Swagger UI available at `http://localhost:5000/swagger`.

In development mode, seed data (events, tax rates, ticket types) is loaded automatically on startup.

### 3. Frontend Setup

```bash
cd frontend

# Install dependencies
bun install

# Start development server
bun dev
```

The frontend will start on `http://localhost:5173` and proxy API requests to the backend.

### 4. Verify Everything Works

- Open `http://localhost:5173` — you should see the event listing page
- Open `http://localhost:5000/swagger` — you should see the API documentation
- Navigate to `/admin/events` — you should see the admin interface (dev mode auto-authenticates)

## Project Structure

```
├── src/                              # Backend (.NET 11)
│   ├── DevConfTicketing.Api/         # Minimal API host, endpoints, middleware
│   ├── DevConfTicketing.Application/ # Business logic handlers
│   ├── DevConfTicketing.Domain/      # Domain models (Event, Order, TicketType, etc.)
│   ├── DevConfTicketing.Infrastructure/ # Cosmos DB, telemetry, repositories
│   └── DevConfTicketing.Tests/       # xUnit tests
├── frontend/                         # Frontend (React 19 + TypeScript)
│   ├── src/
│   │   ├── components/               # Reusable UI components
│   │   ├── hooks/                    # Custom React hooks
│   │   ├── lib/                      # Utilities (API client, auth, telemetry)
│   │   ├── routes/                   # Page components
│   │   └── types/                    # TypeScript type definitions
│   └── e2e/                          # Playwright E2E tests
├── docs/                             # Architecture & planning docs
│   └── attendee-tasks/               # Detailed task descriptions per attendee
├── infra/                            # Bicep IaC for Azure deployment
├── scripts/                          # Setup and utility scripts
└── .github/
    ├── workflows/                    # CI/CD pipelines
    ├── agents/                       # Custom Copilot agent definitions
    └── instructions/                 # Path-specific Copilot instructions
```

## Running Tests

```bash
# Backend tests
dotnet test src/DevConfTicketing.slnx

# Frontend unit tests
cd frontend && bun test

# Frontend E2E tests (requires running backend + frontend)
cd frontend && bunx playwright test
```

## Building & Running with Docker

```bash
# Backend
docker build -t devconf-ticketing-api -f src/DevConfTicketing.Api/Dockerfile .

# Frontend
docker build -t devconf-ticketing-frontend -f frontend/Dockerfile ./frontend
```

## Admin Authentication

In **development** (no Azure AD configured), the frontend uses a mock auth provider that auto-authenticates as an admin user.

In **production**, admin access requires Microsoft Entra ID. Configure:
- `AzureAd__Instance`, `AzureAd__TenantId`, `AzureAd__ClientId` for the backend
- `VITE_AZURE_AD_CLIENT_ID`, `VITE_AZURE_AD_TENANT_ID`, `VITE_AZURE_AD_API_SCOPE` for the frontend

## Documentation

- [Workshop Overview](docs/00-workshop-overview.md)
- [Architecture Plan](docs/01-architecture-plan.md)
- [Custom Agents & Skills](docs/02-custom-agents-and-skills.md)
- [Backend Plan](docs/03-backend-plan.md)
- [Frontend Plan](docs/04-frontend-plan.md)
- [Azure Infrastructure](docs/05-azure-infrastructure.md)
- [Implementation Roadmap](docs/06-implementation-roadmap.md)
- [Deployment Guide](docs/deployment-guide.md)

## Workshop Attendee Tasks

Each attendee works on one independent feature:

1. [Stripe Payment, Refunds & Cancellations](docs/attendee-tasks/task-1-stripe-payment.md)
2. [Invoice, Tax Calculation & Cancellation Invoices](docs/attendee-tasks/task-2-invoice-tax-engine.md)
3. [Customer Auth & Account Management](docs/attendee-tasks/task-3-customer-auth.md)
4. [Ticket PDF, QR-Codes & Email Notifications](docs/attendee-tasks/task-4-email-notifications.md)
5. [Check-in & Ticket Scanning](docs/attendee-tasks/task-5-checkin-scanning.md)
6. [Admin Dashboard, KPIs & Data Export](docs/attendee-tasks/task-6-admin-dashboard-kpis.md)

## License

See [LICENSE](LICENSE) for details.