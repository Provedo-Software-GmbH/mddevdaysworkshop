# Deployment Guide

This guide covers the manual steps required to deploy the DevConf Ticketing infrastructure.

## Progress

- [x] **Step 1a** — App Registration created (`a47bf4df-56e3-42c0-b2ab-4a2b119649e4`)
- [x] **Step 1b** — Mail.Send permission assigned to managed identity
- [x] **Step 2** — Configure GitHub Actions secrets *(needed for CI/CD pipelines — not yet done)*
- [x] **Step 3** — Create `production` GitHub Environment *(needed for CI/CD pipelines — not yet done)*
- [x] **Step 4** — Grant admin consent in Azure Portal *(needed for auth to work — not yet done)*
- [x] **Step 5** — Assign users to Admin/EventManager roles *(done by user)*
- [x] **Infrastructure deployed** — Bicep deployed successfully via Copilot agent
- [ ] **Step 6** — Merge PR to `main` and set up CI/CD secrets so future deploys are automated

## Deployed Infrastructure

The following resources were deployed on **2026-05-18**:

| Resource | URL / Endpoint |
| -------- | -------------- |
| **Backend API** | `https://tickets.v2.api.devconf.nrw` |
| **Frontend** | `https://tickets.v2.devconf.nrw` |
| **Backend API** (default) | `https://devconf-ticketing-api.whitetree-c05b1258.westeurope.azurecontainerapps.io` |
| **Frontend** (default) | `https://devconf-ticketing-frontend.whitetree-c05b1258.westeurope.azurecontainerapps.io` |
| **Cosmos DB** | `https://devconf-ticketing-db.documents.azure.com:443/` |

> Both container apps scale to zero when idle — first request after idle may take a few seconds.
> Custom domains use the `devconf-nrw-wildcard` certificate uploaded to the Container Apps Environment.

## Remaining Manual Steps (for you)

The following steps are needed for **CI/CD automation** and **auth to work in production**. The infrastructure is already deployed, but these are required before merging to `main`:

### 1. Configure GitHub Actions Secrets ⬜

In your repo: **Settings → Secrets and variables → Actions**, add these repository secrets:

| Secret                   | Value                                                        |
| ------------------------ | ------------------------------------------------------------ |
| `AZURE_CLIENT_ID`        | The `GithubActionDeployment` service principal's client ID   |
| `AZURE_TENANT_ID`        | `c6981722-a1fb-46e1-8b25-b5384a0029ca`                      |
| `AZURE_SUBSCRIPTION_ID`  | `aeff9418-2cbd-4197-bc80-c85cd2d851d9`                      |
| `ENTRA_ID_CLIENT_ID`     | `a47bf4df-56e3-42c0-b2ab-4a2b119649e4`                      |

> **How to find the GithubActionDeployment client ID:**
>
> ```powershell
> az ad sp list --display-name GithubActionDeployment --query '[0].appId' -o tsv
> ```

### 2. Create the GitHub Environment ⬜

The deploy pipeline references `environment: production`.

1. Go to repo **Settings → Environments**
2. Click **New environment**
3. Name it `production`
4. Optionally add deployment protection rules (required reviewers, wait timer, etc.)

### 3. Grant Admin Consent ⬜

In the Azure Portal:
1. Go to **Entra ID → App registrations → DevConfTicketing-API**
2. Under **API permissions**, click **Grant admin consent for [your tenant]**

### 4. Merge PR to `main` ⬜

Once the above is done:
1. Merge this PR to `main`
2. The **CI** pipeline will build, test, and push Docker images to `siaconsulting.azurecr.io`
3. The **Deploy** pipeline will deploy the Bicep infrastructure to resource group `nrwdevconf`

You can also trigger the deploy manually via **Actions → Deploy Infrastructure → Run workflow**.

## Architecture Overview

| Resource                  | Name                          | Resource Group   | Location             |
| ------------------------- | ----------------------------- | ---------------- | -------------------- |
| Container Apps Environment| `nrwdevconf-env`              | `nrwdevconf`     | West Europe          |
| Managed Identity          | `nrwdevconf_mi`               | `nrwdevconf`     | Germany West Central |
| Key Vault                 | `nrwdevconf`                  | `nrwdevconf`     | Germany West Central |
| Log Analytics             | `nrwdevconflogs`              | `nrwdevconf`     | Germany West Central |
| Cosmos DB                 | `devconf-ticketing-db`        | `nrwdevconf`     | Germany West Central |
| Application Insights      | `devconf-ticketing-insights`  | `nrwdevconf`     | Germany West Central |
| Backend Container App     | `devconf-ticketing-api`       | `nrwdevconf`     | West Europe          |
| Frontend Container App    | `devconf-ticketing-frontend`  | `nrwdevconf`     | West Europe          |
| Container Registry        | `siaconsulting`               | `sia-consulting` | Germany West Central |

All Azure resources use **managed identity** (no passwords/keys). Cosmos DB has local auth disabled (`disableLocalAuth: true`). Both container apps scale to zero when idle.
