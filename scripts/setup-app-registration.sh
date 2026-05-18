#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────────
# setup-app-registration.sh
#
# Creates or updates the Entra ID App Registration for the DevConf
# Ticketing backend API. Outputs the appId and sets up:
#   - API scope (api://devconf-ticketing/access_as_admin)
#   - App roles: Admin, EventManager
#   - Required API permissions: User.Read
# ──────────────────────────────────────────────────────────────────────
set -euo pipefail

APP_DISPLAY_NAME="DevConfTicketing-API"
API_IDENTIFIER_URI="api://devconf-ticketing"

echo "=== Setting up App Registration: ${APP_DISPLAY_NAME} ==="

# Check if app registration already exists
EXISTING_APP_ID=$(az ad app list --display-name "${APP_DISPLAY_NAME}" --query '[0].appId' -o tsv 2>/dev/null || true)

if [ -n "${EXISTING_APP_ID}" ] && [ "${EXISTING_APP_ID}" != "None" ]; then
  echo "App Registration already exists with appId: ${EXISTING_APP_ID}"
  APP_ID="${EXISTING_APP_ID}"
else
  echo "Creating new App Registration..."
  APP_ID=$(az ad app create \
    --display-name "${APP_DISPLAY_NAME}" \
    --sign-in-audience "AzureADMyOrg" \
    --query 'appId' -o tsv)
  echo "Created App Registration with appId: ${APP_ID}"
fi

# Set identifier URI
echo "Setting identifier URI: ${API_IDENTIFIER_URI}"
az ad app update --id "${APP_ID}" \
  --identifier-uris "${API_IDENTIFIER_URI}" 2>/dev/null || true

# Define app roles (Admin and EventManager)
echo "Configuring app roles..."
az ad app update --id "${APP_ID}" \
  --app-roles '[
    {
      "allowedMemberTypes": ["User"],
      "description": "Full admin access to all DevConf Ticketing features",
      "displayName": "Admin",
      "isEnabled": true,
      "value": "Admin",
      "id": "'"$(cat /proc/sys/kernel/random/uuid)"'"
    },
    {
      "allowedMemberTypes": ["User"],
      "description": "Can manage events, ticket types, and view orders",
      "displayName": "EventManager",
      "isEnabled": true,
      "value": "EventManager",
      "id": "'"$(cat /proc/sys/kernel/random/uuid)"'"
    }
  ]'

# Define API scope
echo "Configuring API scope..."
SCOPE_ID=$(cat /proc/sys/kernel/random/uuid)
az ad app update --id "${APP_ID}" \
  --set "api={\"oauth2PermissionScopes\":[{\"adminConsentDescription\":\"Access DevConf Ticketing API as admin\",\"adminConsentDisplayName\":\"Access DevConf Ticketing API\",\"id\":\"${SCOPE_ID}\",\"isEnabled\":true,\"type\":\"Admin\",\"value\":\"access_as_admin\"}]}"

# Add User.Read delegated permission (Microsoft Graph)
echo "Adding Microsoft Graph User.Read permission..."
# Microsoft Graph appId: 00000003-0000-0000-c000-000000000000
# User.Read permission ID: e1fe6dd8-ba31-4d61-89e7-88639da4683d
az ad app permission add --id "${APP_ID}" \
  --api "00000003-0000-0000-c000-000000000000" \
  --api-permissions "e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope" 2>/dev/null || true

# Ensure a service principal exists for the app
echo "Ensuring service principal exists..."
az ad sp show --id "${APP_ID}" &>/dev/null || az ad sp create --id "${APP_ID}"

echo ""
echo "=== App Registration Setup Complete ==="
echo "App ID (Client ID): ${APP_ID}"
echo "Identifier URI:     ${API_IDENTIFIER_URI}"
echo "API Scope:          ${API_IDENTIFIER_URI}/access_as_admin"
echo ""
echo "Next steps:"
echo "  1. Set entraIdClientId=${APP_ID} in your Bicep parameters"
echo "  2. Grant admin consent in Azure Portal if needed"
echo "  3. Assign users to Admin/EventManager roles in Enterprise Applications"
