# Contributing to DevConf Ticketing

This guide covers the development workflow for the DevConf Ticketing workshop project.

## Getting Started

1. **Fork** the repository to your GitHub account (or your organization)
2. **Clone** your fork locally
3. **Set up** the development environment (see [README.md](README.md#quick-start))
4. **Create a branch** for your feature: `git checkout -b feature/your-feature-name`

## Development Environment

### Required Tools

| Tool | Version | Purpose |
| ---- | ------- | ------- |
| .NET SDK | 11.x (Preview) | Backend API |
| Bun | Latest | Frontend package manager & runtime |
| Node.js | 22+ | Required by some tooling |
| Docker | Latest | Containerization (optional) |
| Azure Cosmos DB Emulator | Latest | Local database |

### IDE Recommendations

- **VS Code** with the following extensions:
  - C# Dev Kit
  - GitHub Copilot + Copilot Chat
  - Tailwind CSS IntelliSense
  - ESLint
  - Prettier
- **Rider** or **Visual Studio 2025** for backend work

### GitHub Copilot Agent Setup

This project is designed for use with GitHub Copilot Agents. The repository includes:

- **Custom agents** in `.github/agents/` — specialized for backend, frontend, testing, and KPIs
- **Skills** for Azure Log Analytics, Cosmos DB queries, Stripe integration, German tax calculation, and accessibility checking
- **Path-specific instructions** in `.github/instructions/` that guide Copilot based on which files you're editing

When working with Copilot:
- Assign your GitHub Issue to the Copilot agent
- Let the agent create a PR against your fork
- Review the PR, iterate as needed, then merge

## Code Style & Conventions

### Backend (C# / .NET)

- **File-scoped namespaces** — one type per file
- **Minimal APIs** — no controllers
- **Primary constructors** where appropriate
- **Pattern matching** — switch expressions, property patterns
- **Nullable reference types** — always enabled
- All models annotated with `[Description]` and `[JsonPropertyName]` attributes
- Use `System.Diagnostics.Activity` for tracing (not `TelemetryClient`)
- Use `System.Diagnostics.Metrics` for custom metrics
- See `.editorconfig` for full naming rules

### Frontend (TypeScript / React)

- **Strict TypeScript** — no `any` types
- **Functional components** only (React 19)
- **TanStack Query** for server state
- **shadcn/ui** for UI components
- **Tailwind CSS** for styling (no inline styles, no CSS modules)
- Custom hooks in `hooks/` directory
- Types in `types/` directory
- WCAG 2.1 AA accessibility compliance

### Commit Messages

Use conventional commit format:

```
feat: add stripe webhook endpoint
fix: correct tax calculation for 7% rate
docs: update README with deployment steps
test: add order creation validation tests
refactor: extract tax calculation service
```

## Building & Testing

### Backend

```bash
# Build
dotnet build src/DevConfTicketing.slnx

# Run tests
dotnet test src/DevConfTicketing.slnx

# Run with hot reload
dotnet watch --project src/DevConfTicketing.Api
```

### Frontend

```bash
cd frontend

# Install dependencies
bun install

# Lint
bun lint

# Unit tests
bun test

# Build for production
bun run build

# E2E tests (requires backend running)
bunx playwright test
```

## Architecture Overview

The application uses a layered architecture:

```
API Layer (Endpoints, Middleware)
    ↓
Application Layer (Handlers, Services)
    ↓
Domain Layer (Models, Enums)
    ↓
Infrastructure Layer (Cosmos DB, Telemetry)
```

### Key Patterns

- **Repository Pattern** — `CosmosDbService` with typed repositories per entity
- **Handler Pattern** — one handler class per operation (CreateEventHandler, etc.)
- **Endpoint Groups** — related endpoints grouped in static classes (EventEndpoints, etc.)
- **Telemetry Throughout** — every handler and middleware emits spans and metrics

## Working on Your Assigned Task

Each attendee has a dedicated GitHub Issue with:
- Detailed acceptance criteria
- A task checklist to follow
- References to relevant existing code

### Workflow

1. Read your assigned issue thoroughly
2. Explore the referenced existing code to understand patterns
3. Use the Copilot agent to implement — assign the issue to Copilot
4. Review the generated PR
5. Iterate: comment on the PR with feedback, let Copilot fix issues
6. Once satisfied, request a review from the workshop leader

### Tips

- **Start small**: implement one checklist item at a time
- **Follow existing patterns**: look at how Events/TicketTypes/TaxRates are implemented
- **Use the agents**: the custom agents know the project conventions
- **Test as you go**: run `dotnet test` and `bun test` frequently
- **Check the docs**: `docs/attendee-tasks/` has detailed task descriptions

## Deployment

The CI/CD pipeline handles deployment automatically:
- **CI** (`ci.yml`): Runs on every push/PR — builds, tests, lints
- **Deploy** (`deploy.yml`): Deploys to Azure Container Apps on merge to `main`

For manual deployment details, see [docs/deployment-guide.md](docs/deployment-guide.md).

## Troubleshooting

### Cosmos DB Emulator won't start
- Ensure Docker is running (the emulator runs in a container on Linux/Mac)
- Check port 8081 is available
- Try the [Azure Cosmos DB Emulator documentation](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator)

### Frontend proxy not working
- Ensure the backend is running on port 5000
- Check `frontend/vite.config.ts` proxy configuration
- Look for CORS errors in the browser console

### Authentication issues in development
- Without Azure AD configured, the app auto-authenticates in dev mode
- If you see auth errors, ensure `VITE_AZURE_AD_CLIENT_ID` is NOT set (or set correctly)

### Build failures
- Run `dotnet restore src/DevConfTicketing.slnx` to restore NuGet packages
- Run `bun install` in the frontend directory
- Ensure you're using .NET 11 Preview SDK: `dotnet --version`
