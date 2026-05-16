# Azure Infrastructure Plan

## Resource Overview

All resources optimized for cost efficiency using serverless/flexible SKUs.

### Resource Group

- **Name**: `rg-devconf-ticketing-{env}` (e.g., `rg-devconf-ticketing-prod`)
- **Region**: `West Europe` (Frankfurt/Netherlands)

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

### Database — Azure Cosmos DB

| Resource                    | Configuration                          |
| --------------------------- | -------------------------------------- |
| Cosmos DB Account           | Serverless capacity mode               |
| API                         | NoSQL (Core SQL)                       |
| Consistency                 | Session (default, good balance)        |
| Backup                      | Continuous (7 days, free with serverless) |

**Cost**: ~€0-5/month for low traffic, scales with RU consumption.

**Containers** (auto-created by app):
- `events` (PK: `/id`)
- `ticket-types` (PK: `/eventId`)
- `tax-rates` (PK: `/countryCode`)
- `orders` (PK: `/eventId`)
- `customers` (PK: `/id`)
- `sessions` (PK: `/eventId`)
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
- Connection string injected via Container App secrets
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

**Cost**: ~€0.03/10,000 operations. Negligible.

**Secrets stored**:
- `Stripe--SecretKey`
- `Stripe--PublishableKey`
- `Stripe--WebhookSecret`
- `CosmosDb--ConnectionString`
- `AzureCommunicationServices--ConnectionString`

### Container Registry — Azure Container Registry

| Resource              | Configuration              |
| --------------------- | -------------------------- |
| Azure Container Registry | Basic tier              |

**Cost**: ~€4.50/month

### Email — Azure Communication Services (Optional)

| Resource                          | Configuration              |
| --------------------------------- | -------------------------- |
| Azure Communication Services      | Pay-as-you-go             |
| Email Communication Service       | Free domain included       |

**Cost**: First 1000 emails/month free, then ~€0.00025/email.

---

## Estimated Monthly Cost Summary

| Service                    | Estimated Cost (Low Traffic) |
| -------------------------- | ---------------------------- |
| Container Apps             | €5-15                        |
| Cosmos DB (Serverless)     | €2-5                         |
| Container Registry (Basic) | €4.50                        |
| Application Insights       | €0 (under 5 GB)             |
| Key Vault                  | ~€0.03                       |
| Entra External ID          | €0 (under 50k MAU)          |
| Communication Services     | €0 (under 1000 emails)      |
| **Total**                  | **~€12-25/month**            |

---

## Deployment Architecture

```
GitHub Actions
    │
    ├── Build Backend → Docker Image → ACR
    ├── Build Frontend → Docker Image → ACR
    │
    └── Deploy
        ├── Container App (Backend) ← ACR Image
        ├── Container App (Frontend) ← ACR Image
        └── Cosmos DB (auto-provision containers)
```

### GitHub Actions Secrets Required

```
AZURE_CREDENTIALS          # Service Principal JSON
ACR_LOGIN_SERVER           # e.g., devconfticketingacr.azurecr.io
ACR_USERNAME               # ACR admin username
ACR_PASSWORD               # ACR admin password
STRIPE_SECRET_KEY          # Stripe test/live secret key
STRIPE_PUBLISHABLE_KEY     # Stripe test/live publishable key
STRIPE_WEBHOOK_SECRET      # Stripe webhook signing secret
```

### Infrastructure as Code

For production deployment, use **Bicep** templates:

```
infra/
├── main.bicep                    # Main template
├── modules/
│   ├── container-apps.bicep      # Container Apps Environment + Apps
│   ├── cosmos-db.bicep           # Cosmos DB Account
│   ├── container-registry.bicep  # ACR
│   ├── key-vault.bicep           # Key Vault
│   ├── monitoring.bicep          # App Insights + Log Analytics
│   └── communication.bicep       # Communication Services
└── parameters/
    ├── dev.bicepparam
    └── prod.bicepparam
```

> **Note**: Bicep templates are NOT part of the workshop scope but should be created for production deployment afterwards.

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
