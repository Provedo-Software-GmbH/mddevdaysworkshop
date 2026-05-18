# Deployment Guide

This guide covers the manual steps required to deploy the DevConf Ticketing infrastructure.

## Prerequisites

- Azure CLI installed and logged in (`az login`)
- An account with **Global Administrator** or **Privileged Role Administrator** for the Entra ID scripts
- Access to the GitHub repository settings

## Step 0: Clean Up Broken App Registration (one-time)

If you previously ran the script and it failed with JSON errors, the app registration exists but has no roles or scopes. Delete it first so the script can recreate it cleanly:

```powershell
az login   # log in with an admin account

# Find the broken app registration
$BrokenAppId = az ad app list --display-name DevConfTicketing-API --query '[0].appId' -o tsv
Write-Host "Found app: $BrokenAppId"

# Delete it
az ad app delete --id $BrokenAppId

# Verify it's gone
az ad app list --display-name DevConfTicketing-API --query '[].appId' -o tsv
# (should return nothing)
```

## Step 1: Run the Entra ID Setup Scripts

The deployment service principal only has Azure resource permissions — it **cannot** create App Registrations or assign Graph API roles. These scripts must be run by an Entra ID admin.

### 1a. Create the App Registration

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

**Save the App ID (Client ID) from the output** — you'll need it in Step 2.

### 1b. Assign Graph API Mail.Send Permission

```powershell
.\scripts\Setup-GraphPermissions.ps1
# Or with custom values:
.\scripts\Setup-GraphPermissions.ps1 -ManagedIdentityName "nrwdevconf_mi" -ResourceGroup "nrwdevconf"
```

This assigns the `Mail.Send` application permission to the managed identity so the backend can send ticket confirmation emails via Microsoft Graph API.

> **Note:** This cannot be done via Bicep — it requires direct Microsoft Graph API calls.

## Step 2: Configure GitHub Actions Secrets

In your repo: **Settings → Secrets and variables → Actions**, add these repository secrets:

| Secret                   | Value                                                        |
| ------------------------ | ------------------------------------------------------------ |
| `AZURE_CLIENT_ID`        | The `GithubActionDeployment` service principal's client ID   |
| `AZURE_TENANT_ID`        | Your Entra ID tenant ID                                      |
| `AZURE_SUBSCRIPTION_ID`  | Your Azure subscription ID                                   |
| `ENTRA_ID_CLIENT_ID`     | The App ID output from `Setup-AppRegistration.ps1` (Step 1a) |

## Step 3: Create the GitHub Environment

The deploy pipeline references `environment: production`.

1. Go to repo **Settings → Environments**
2. Click **New environment**
3. Name it `production`
4. Optionally add deployment protection rules (required reviewers, wait timer, etc.)

## Step 4: Grant Admin Consent (if needed)

In the Azure Portal:
1. Go to **Entra ID → App registrations → DevConfTicketing-API**
2. Under **API permissions**, click **Grant admin consent for [your tenant]**

## Step 5: Assign Users to Roles

In the Azure Portal:
1. Go to **Entra ID → Enterprise applications → DevConfTicketing-API**
2. Under **Users and groups**, assign users to the `Admin` or `EventManager` roles

## Step 6: Merge and Deploy

Once the above is configured:
1. Merge the PR to `main`
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
