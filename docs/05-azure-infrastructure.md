# Azure Infrastructure Plan

## Resource Overview

All resources optimized for cost efficiency using serverless/flexible SKUs.
**All Azure service access uses Managed Identities** — no connection strings or passwords in configuration.

### Resource Group

- **Name**: `rg-devconf-ticketing-{env}` (e.g., `rg-devconf-ticketing-prod`)
- **Region**: `West Europe` (Frankfurt/Netherlands)

### Identity — Managed Identities

| Resource                        | Configuration                    |
| ------------------------------- | -------------------------------- |
| User-Assigned Managed Identity  | Shared by backend Container App  |

**Cost**: Free.

The backend Container App uses a **User-Assigned Managed Identity** to authenticate to all Azure services:
- **Cosmos DB** — RBAC role: `Cosmos DB Built-in Data Contributor`
- **Key Vault** — Access policy or RBAC role: `Key Vault Secrets User`
- **Application Insights** — Connection via managed identity (no instrumentation key in config)
- **Microsoft Graph API** — `Mail.Send` application permission assigned directly to the managed identity (no App Registration client secret needed)

> **No connection strings or passwords are stored as Container App env vars or secrets.** All secrets are accessed via the ASP.NET Key Vault configuration provider at startup.

### Compute — Azure Container Apps

| Resource                        | Configuration                    |
| ------------------------------- | -------------------------------- |
| Container Apps Environment      | Consumption plan (serverless)    |
| Backend Container App           | Min replicas: 0, Max: 5         |
| Frontend Container App          | Min replicas: 0, Max: 3         |

**Cost**: Pay only for active usage. ~€0 when idle, ~€5-15/month with moderate traffic.

- Ingress: HTTPS with custom domain
- Backend: Internal + External ingress (API accessible from frontend and externally for webhooks)
- Frontend: External ingress only
- Scaling rules: HTTP concurrent requests
- **Managed Identity**: User-assigned managed identity attached to backend Container App
- **Key Vault reference**: The backend uses `Azure.Extensions.AspNetCore.Configuration.Secrets` to load all config from Key Vault at startup (not Container App secrets/env vars)

### Database — Azure Cosmos DB

| Resource                    | Configuration                          |
| --------------------------- | -------------------------------------- |
| Cosmos DB Account           | Serverless capacity mode               |
| API                         | NoSQL (Core SQL)                       |
| Consistency                 | Session (default, good balance)        |
| Backup                      | Continuous (7 days, free with serverless) |
| **Authentication**          | **Microsoft Entra ID (Managed Identity)** — no connection string |

**Cost**: ~€0-5/month for low traffic, scales with RU consumption.

**Access**: The backend authenticates to Cosmos DB using its managed identity with the `Cosmos DB Built-in Data Contributor` RBAC role. No connection string needed.

**Containers** (auto-created by app):
- `events` (PK: `/id`)
- `ticket-types` (PK: `/eventId`)
- `tax-rates` (PK: `/countryCode`)
- `orders` (PK: `/eventId`)
- `customers` (PK: `/id`)
- `vouchers` (PK: `/eventId`)
- `checkin-lists` (PK: `/eventId`)
- `checkin-records` (PK: `/eventId`)
- `invoices` (PK: `/orderId`)

### Authentication — Microsoft Entra ID

| Resource                              | Configuration                    |
| ------------------------------------- | -------------------------------- |
| App Registration (Admin API)          | Single tenant, Web app           |
| App Registration (Frontend SPA)       | Single tenant, SPA               |
| Entra External Identities Tenant      | Free tier (up to 50k MAU)       |
| External ID App Registration          | Customer-facing auth             |

**Cost**: Free tier covers workshop and initial production use.

**Admin App Registration**:
- Redirect URI: `https://admin.devconf-ticketing.de/callback`
- API Permissions: `User.Read`
- App Roles: `Admin`, `EventManager`

**Customer External ID**:
- Self-service sign-up enabled
- Email + Password (local accounts)
- Optional: Social logins (Google, GitHub)
- Redirect URI: `https://devconf-ticketing.de/auth/callback`

### Monitoring — Application Insights + Log Analytics

| Resource                      | Configuration                      |
| ----------------------------- | ---------------------------------- |
| Application Insights          | Workspace-based                    |
| Log Analytics Workspace       | Pay-as-you-go (free up to 5GB/month) |

**Cost**: Free for up to 5 GB/month of log data. Easily sufficient for this app.

**Configuration**:
- OpenTelemetry SDK with Azure Monitor exporter (`Azure.Monitor.OpenTelemetry.AspNetCore`) replaces the legacy Application Insights SDK
- Application Insights connection string stored in Key Vault, loaded via ASP.NET Key Vault config provider
- `System.Diagnostics.Activity` for distributed tracing, `System.Diagnostics.Metrics` for custom metrics
- `ActivitySource("DevConfTicketing")` and `Meter("DevConfTicketing")` registered as listeners in the OpenTelemetry pipeline
- Auto-instrumentation for ASP.NET Core requests and outgoing HTTP calls
- Service metadata: service.name, service.version, service.namespace as OpenTelemetry resource attributes
- Custom metrics for business KPIs
- Availability tests (optional)
- Alert rules:
  - 5xx error rate > 1% → Email alert
  - Response time p95 > 2s → Email alert
  - Failed requests > 10/min → Email alert

### Secrets — Azure Key Vault

| Resource              | Configuration              |
| --------------------- | -------------------------- |
| Azure Key Vault       | Standard tier              |
| **Access**            | **Managed Identity (RBAC: Key Vault Secrets User)** |

**Cost**: ~€0.03/10,000 operations. Negligible.

**Access Pattern**: The backend uses the **ASP.NET Key Vault configuration provider** (`Azure.Extensions.AspNetCore.Configuration.Secrets`) to load secrets at startup. The managed identity authenticates to Key Vault — no access keys or connection strings needed.

```csharp
// Program.cs — Key Vault configuration
builder.Configuration.AddAzureKeyVault(
    new Uri("https://kv-devconf-ticketing.vault.azure.net/"),
    new DefaultAzureCredential());
```

**Secrets stored**:
- `Stripe--SecretKey`
- `Stripe--PublishableKey`
- `Stripe--WebhookSecret`
- `CosmosDb--AccountEndpoint` (only the endpoint URL, auth via managed identity)
- `GraphApi--SenderEmail` (shared mailbox or user for sending, e.g. `tickets@devconf-ticketing.de`)

### Container Registry — Existing (Reuse)

> **Note**: We reuse an **existing Azure Container Registry** — no new ACR is provisioned.

| Resource              | Configuration              |
| --------------------- | -------------------------- |
| Azure Container Registry | **Existing** (already provisioned) |

**Access**: The Container Apps Environment pulls images from the existing ACR. Authentication via managed identity or admin credentials (depending on existing ACR setup).

### Email — Microsoft Graph API

| Resource                          | Configuration                            |
| --------------------------------- | ---------------------------------------- |
| **Microsoft Graph API**           | Application permission (`Mail.Send`) via Managed Identity |

**Cost**: Free (included in Microsoft 365 / Entra ID licensing).

**How it works**:
- The `Mail.Send` application permission is granted directly to the **User-Assigned Managed Identity** (via Microsoft Graph `appRoleAssignment`) — no App Registration client secret needed
- Backend authenticates using `DefaultAzureCredential` (same managed identity used for all Azure services)
- Sends emails via `POST /v1.0/users/{sender}/sendMail` endpoint
- Sender is a shared mailbox (e.g., `tickets@devconf-ticketing.de`) or a licensed user
- Supports HTML emails with attachments (ticket PDFs)
- **No client secrets anywhere** — fully managed identity based

**Advantages over Azure Communication Services**:
- No additional Azure resource to provision
- Full control over sender identity (own domain)
- Rich HTML email with attachments supported natively
- Integrates with existing Microsoft 365 infrastructure

**Setup — Assigning `Mail.Send` to Managed Identity**:
> Granting application permissions to a managed identity is not available through the Azure Portal UI. Use PowerShell or the Microsoft Graph API directly:
> ```powershell
> # Grant Mail.Send to managed identity via PowerShell
> $graphApp = Get-MgServicePrincipal -Filter "appId eq '00000003-0000-0000-c000-000000000000'" # Microsoft Graph
> $mailSendRole = $graphApp.AppRoles | Where-Object { $_.Value -eq "Mail.Send" }
> $managedIdentitySp = Get-MgServicePrincipal -Filter "displayName eq '<managed-identity-name>'"
> New-MgServicePrincipalAppRoleAssignment -ServicePrincipalId $managedIdentitySp.Id `
>   -PrincipalId $managedIdentitySp.Id -ResourceId $graphApp.Id -AppRoleId $mailSendRole.Id
> ```
> Requires **Global Administrator** or **Privileged Role Administrator** consent.

---

## Estimated Monthly Cost Summary

| Service                    | Estimated Cost (Low Traffic) |
| -------------------------- | ---------------------------- |
| Container Apps             | €5-15                        |
| Cosmos DB (Serverless)     | €2-5                         |
| Container Registry         | €0 (existing, already paid)  |
| Application Insights       | €0 (under 5 GB)             |
| Key Vault                  | ~€0.03                       |
| Entra External ID          | €0 (under 50k MAU)          |
| Graph API (Email)          | €0 (included in M365)       |
| Managed Identity           | €0 (free)                    |
| **Total**                  | **~€7-20/month**             |

---

## Deployment Architecture

```
GitHub Actions
    │
    ├── Build Backend → Docker Image → Existing ACR
    ├── Build Frontend → Docker Image → Existing ACR
    │
    └── Deploy
        ├── Container App (Backend) ← ACR Image + Managed Identity
        ├── Container App (Frontend) ← ACR Image
        └── Cosmos DB (auto-provision containers, RBAC via Managed Identity)
```

### GitHub Actions Secrets Required

```
AZURE_CREDENTIALS          # Service Principal JSON (for deployment only)
ACR_LOGIN_SERVER           # e.g., existingacr.azurecr.io (existing registry)
ACR_USERNAME               # ACR admin username (or use managed identity for push)
ACR_PASSWORD               # ACR admin password
STRIPE_SECRET_KEY          # Stripe test/live secret key (stored in Key Vault, used for initial setup)
STRIPE_PUBLISHABLE_KEY     # Stripe test/live publishable key
STRIPE_WEBHOOK_SECRET      # Stripe webhook signing secret
```

> **Note**: At runtime, the backend does NOT read secrets from env vars or GitHub Secrets. It reads them from **Azure Key Vault** via the ASP.NET configuration provider + managed identity.

### Infrastructure as Code

For production deployment, use **Bicep** templates:

```
infra/
├── main.bicep                    # Main template
├── modules/
│   ├── container-apps.bicep      # Container Apps Environment + Apps
│   ├── cosmos-db.bicep           # Cosmos DB Account + RBAC assignments
│   ├── key-vault.bicep           # Key Vault + access policies
│   ├── monitoring.bicep          # App Insights + Log Analytics
│   ├── managed-identity.bicep    # User-Assigned Managed Identity + role assignments
│   └── graph-app-registration.bicep # App Registration for Graph API (manual/script)
└── parameters/
    ├── dev.bicepparam
    └── prod.bicepparam
```

> **Note**: Bicep templates are NOT part of the workshop scope but should be created for production deployment afterwards. The ACR module is removed since we reuse an existing registry.

---

## Local Development Setup

For the workshop, attendees run everything locally:

```bash
# Backend
cd src/DevConfTicketing.Api
dotnet run                    # Starts on http://localhost:5000

# Frontend (separate terminal)
cd frontend
bun dev                       # Starts on http://localhost:5173, proxies /api to :5000

# Cosmos DB
# Option A: Azure Cosmos DB Emulator (Windows/Docker)
# Option B: Free-tier Cosmos DB account in Azure (recommended for workshop)
```

### Cosmos DB for Development

**Recommended**: Use a shared Cosmos DB Serverless account in Azure for the workshop.
- All attendees connect to the same Cosmos DB
- Each attendee uses a database prefix (e.g., `attendee1-events`, `attendee2-events`)
- Or: Each attendee has their own Cosmos DB database within the same account

**Alternative**: Cosmos DB Emulator
- Available for Windows and Linux (Docker)
- Connection string: `AccountEndpoint=https://localhost:8081/;AccountKey=...`
