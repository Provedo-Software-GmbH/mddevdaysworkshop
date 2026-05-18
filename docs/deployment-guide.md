# Deployment Guide

This guide covers the manual steps required to deploy the DevConf Ticketing infrastructure.

## Progress

- [x] **Step 1a** — App Registration created (`a47bf4df-56e3-42c0-b2ab-4a2b119649e4`)
- [x] **Step 1b** — Mail.Send permission assigned to managed identity
- [ ] **Step 2** — Configure GitHub Actions secrets
- [ ] **Step 3** — Create `production` GitHub Environment
- [ ] **Step 4** — Grant admin consent in Azure Portal
- [ ] **Step 5** — Assign users to Admin/EventManager roles
- [ ] **Step 6** — Merge PR and deploy

## Prerequisites

- Azure CLI installed and logged in (`az login`)
- An account with **Global Administrator** or **Privileged Role Administrator** for the Entra ID scripts
- Access to the GitHub repository settings

## Step 1: Run the Entra ID Setup Scripts ✅

The deployment service principal only has Azure resource permissions — it **cannot** create App Registrations or assign Graph API roles. These scripts must be run by an Entra ID admin.

### 1a. Create the App Registration ✅

```powershell
az login   # log in with an admin account
.\scripts\Setup-AppRegistration.ps1
```

This creates:
- App Registration `DevConfTicketing-API` with identifier URI `api://devconf-ticketing`
- App roles: `Admin`, `EventManager`
- API scope: `access_as_admin`
- Microsoft Graph `User.Read` delegated permission
- A service principal for the app

> **Done.** App ID: `a47bf4df-56e3-42c0-b2ab-4a2b119649e4`

### 1b. Assign Graph API Mail.Send Permission ✅

```powershell
.\scripts\Setup-GraphPermissions.ps1
# Or with custom values:
.\scripts\Setup-GraphPermissions.ps1 -ManagedIdentityName "nrwdevconf_mi" -ResourceGroup "nrwdevconf"
```

This assigns the `Mail.Send` application permission to the managed identity so the backend can send ticket confirmation emails via Microsoft Graph API.

> **Done.** Mail.Send assigned to managed identity `nrwdevconf_mi` (principal `4b5591f5-4d48-4e8e-b6af-a36c3705190c`).

## Step 2: Configure GitHub Actions Secrets ⬜

In your repo: **Settings → Secrets and variables → Actions**, add these repository secrets:

| Secret                   | Value                                                        |
| ------------------------ | ------------------------------------------------------------ |
| `AZURE_CLIENT_ID`        | The `GithubActionDeployment` service principal's client ID   |
| `AZURE_TENANT_ID`        | Your Entra ID tenant ID                                      |
| `AZURE_SUBSCRIPTION_ID`  | Your Azure subscription ID                                   |
| `ENTRA_ID_CLIENT_ID`     | `a47bf4df-56e3-42c0-b2ab-4a2b119649e4`                      |

> **How to find the values you need:**
>
> ```powershell
> # Tenant ID and Subscription ID
> az account show --query '{tenantId:tenantId, subscriptionId:id}' -o table
>
> # GithubActionDeployment service principal client ID
> az ad sp list --display-name GithubActionDeployment --query '[0].appId' -o tsv
> ```

## Step 3: Create the GitHub Environment ⬜

The deploy pipeline references `environment: production`.

1. Go to repo **Settings → Environments**
2. Click **New environment**
3. Name it `production`
4. Optionally add deployment protection rules (required reviewers, wait timer, etc.)

## Step 4: Grant Admin Consent ⬜

In the Azure Portal:
1. Go to **Entra ID → App registrations → DevConfTicketing-API**
2. Under **API permissions**, click **Grant admin consent for [your tenant]**

## Step 5: Assign Users to Roles ⬜

In the Azure Portal:
1. Go to **Entra ID → Enterprise applications → DevConfTicketing-API**
2. Under **Users and groups**, assign users to the `Admin` or `EventManager` roles

## Step 6: Merge and Deploy ⬜

Once Steps 2–5 are done:
1. Merge this PR to `main`
2. The **CI** pipeline will build, test, and push Docker images to `siaconsulting.azurecr.io`
3. The **Deploy** pipeline will deploy the Bicep infrastructure to resource group `nrwdevconf`

You can also trigger the deploy manually via **Actions → Deploy Infrastructure → Run workflow**.

## Architecture Overview

| Resource                  | Name                          | Resource Group   |
| ------------------------- | ----------------------------- | ---------------- |
| Container Apps Environment| `nrwdevconf-env`              | `nrwdevconf`     |
| Managed Identity          | `nrwdevconf_mi`               | `nrwdevconf`     |
| Key Vault                 | `nrwdevconf`                  | `nrwdevconf`     |
| Log Analytics             | `nrwdevconflogs`              | `nrwdevconf`     |
| Cosmos DB                 | `devconf-ticketing-db`        | `nrwdevconf`     |
| Application Insights      | `devconf-ticketing-insights`  | `nrwdevconf`     |
| Backend Container App     | `devconf-ticketing-api`       | `nrwdevconf`     |
| Frontend Container App    | `devconf-ticketing-frontend`  | `nrwdevconf`     |
| Container Registry        | `siaconsulting`               | `sia-consulting` |

All Azure resources use **managed identity** (no passwords/keys). Cosmos DB has local auth disabled (`disableLocalAuth: true`). Both container apps scale to zero when idle.
