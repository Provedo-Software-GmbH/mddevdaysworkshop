# ──────────────────────────────────────────────────────────────────────
# Setup-AppRegistration.ps1
#
# Creates or updates the Entra ID App Registration for the DevConf
# Ticketing backend API. Outputs the appId and sets up:
#   - API scope (api://devconf-ticketing/access_as_admin)
#   - App roles: Admin, EventManager
#   - Required API permissions: User.Read
#
# Prerequisites:
#   - Azure CLI installed and logged in with Entra ID admin permissions
#
# Usage:
#   .\scripts\Setup-AppRegistration.ps1
# ──────────────────────────────────────────────────────────────────────

$ErrorActionPreference = 'Stop'

$AppDisplayName = 'DevConfTicketing-API'
$ApiIdentifierUri = 'api://devconf-ticketing'

Write-Host "=== Setting up App Registration: $AppDisplayName ==="

# Check if app registration already exists
$ExistingAppId = az ad app list --display-name $AppDisplayName --query '[0].appId' -o tsv 2>&1 | Where-Object { $_ -is [string] }

if ($ExistingAppId -and $ExistingAppId -ne 'None') {
    Write-Host "App Registration already exists with appId: $ExistingAppId"
    $AppId = $ExistingAppId
}
else {
    Write-Host 'Creating new App Registration...'
    $AppId = az ad app create `
        --display-name $AppDisplayName `
        --sign-in-audience 'AzureADMyOrg' `
        --query 'appId' -o tsv
    Write-Host "Created App Registration with appId: $AppId"
}

# Set identifier URI
Write-Host "Setting identifier URI: $ApiIdentifierUri"
az ad app update --id $AppId --identifier-uris $ApiIdentifierUri 2>&1 | Out-Null

# Define app roles (Admin and EventManager)
Write-Host 'Configuring app roles...'
$AdminRoleId = [guid]::NewGuid().ToString()
$EventManagerRoleId = [guid]::NewGuid().ToString()

$AppRoles = @"
[
  {
    "allowedMemberTypes": ["User"],
    "description": "Full admin access to all DevConf Ticketing features",
    "displayName": "Admin",
    "isEnabled": true,
    "value": "Admin",
    "id": "$AdminRoleId"
  },
  {
    "allowedMemberTypes": ["User"],
    "description": "Can manage events, ticket types, and view orders",
    "displayName": "EventManager",
    "isEnabled": true,
    "value": "EventManager",
    "id": "$EventManagerRoleId"
  }
]
"@

az ad app update --id $AppId --app-roles $AppRoles

# Define API scope
Write-Host 'Configuring API scope...'
$ScopeId = [guid]::NewGuid().ToString()
$ApiBody = "api={""oauth2PermissionScopes"":[{""adminConsentDescription"":""Access DevConf Ticketing API as admin"",""adminConsentDisplayName"":""Access DevConf Ticketing API"",""id"":""$ScopeId"",""isEnabled"":true,""type"":""Admin"",""value"":""access_as_admin""}]}"
az ad app update --id $AppId --set $ApiBody

# Add User.Read delegated permission (Microsoft Graph)
Write-Host 'Adding Microsoft Graph User.Read permission...'
# Microsoft Graph appId: 00000003-0000-0000-c000-000000000000
# User.Read permission ID: e1fe6dd8-ba31-4d61-89e7-88639da4683d
az ad app permission add --id $AppId `
    --api '00000003-0000-0000-c000-000000000000' `
    --api-permissions 'e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope' 2>&1 | Out-Null

# Ensure a service principal exists for the app
Write-Host 'Ensuring service principal exists...'
$spExists = $null
try { $spExists = az ad sp show --id $AppId 2>&1 } catch { }
if (-not $spExists) {
    az ad sp create --id $AppId
}

Write-Host ''
Write-Host '=== App Registration Setup Complete ==='
Write-Host "App ID (Client ID): $AppId"
Write-Host "Identifier URI:     $ApiIdentifierUri"
Write-Host "API Scope:          $ApiIdentifierUri/access_as_admin"
Write-Host ''
Write-Host 'Next steps:'
Write-Host "  1. Set entraIdClientId=$AppId in your Bicep parameters"
Write-Host '  2. Grant admin consent in Azure Portal if needed'
Write-Host '  3. Assign users to Admin/EventManager roles in Enterprise Applications'
