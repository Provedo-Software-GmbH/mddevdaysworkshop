# Copilot Instructions

These are the **global, project-wide** instructions. Path-specific instructions for backend (`src/`) and frontend (`frontend/`) are in `.github/instructions/`.

## Project Overview

This is a **DevConf Ticketing** application — an event ticketing platform with a .NET 11 Minimal API backend and a React/TypeScript frontend (using Bun as the package manager).

## Code Organization

- **One file per type**: Always create one file per class, record, struct, enum, or interface
- **File-scoped namespaces**: Use file-scoped namespace declarations (backend)

## Model Annotations

All models (domain and DTOs) must be annotated with:
- `[Description("...")]` attribute on all properties
- `[JsonPropertyName("...")]` attribute on all properties

This is required to support a future MCP server.

## Coding Standards

Refer to the `.editorconfig` file in the repository root for all coding and naming guidelines.

## Dependencies and Framework

- Stay on the latest version of .NET (currently .NET 11 preview) and libraries, including preview versions
- Always explicitly reference the latest `Azure.Identity` package so transitive dependencies use our version
- Prefer modern, actively maintained packages

## Agent Routing

When working on this project, delegate to specialized agents:

- **Backend API work** (`src/`): delegate to `backend-api-agent`
- **Frontend components** (`frontend/`): delegate to `frontend-component-agent`
- **Unit tests**: always run `testing-agent` after implementing a feature
- **E2E tests** (frontend workflows): delegate to `e2e-testing-agent`
- **KPI/metrics definition**: **always** run `kpi-agent` after implementing a feature to define and implement KPIs

## Skills

The following skills are available for specialized knowledge:

- `azure-log-analytics-scanner` — scan Application Insights for errors
- `cosmos-db-query-helper` — write efficient Cosmos DB queries
- `stripe-integration-helper` — Stripe payment integration patterns
- `german-tax-calculation` — German VAT and tax rules for event ticketing
- `accessibility-checker` — WCAG 2.1 AA compliance review
