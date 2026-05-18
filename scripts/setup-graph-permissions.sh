#!/usr/bin/env bash
# ──────────────────────────────────────────────────────────────────────
# setup-graph-permissions.sh
#
# Assigns Microsoft Graph application permissions (Mail.Send) directly
# to a user-assigned managed identity's service principal.
#
# This CANNOT be done via Bicep — it requires Microsoft Graph API calls
# to create appRoleAssignments on the managed identity's service principal.
#
# Prerequisites:
#   - Azure CLI logged in with sufficient permissions
#   - The managed identity must already exist
#
# Usage:
#   ./scripts/setup-graph-permissions.sh [managed-identity-name] [resource-group]
# ──────────────────────────────────────────────────────────────────────
set -euo pipefail

MI_NAME="${1:-nrwdevconf_mi}"
MI_RESOURCE_GROUP="${2:-nrwdevconf}"

echo "=== Setting up Graph API permissions for Managed Identity: ${MI_NAME} ==="

# Get the managed identity's principal ID (object ID of its service principal)
echo "Looking up managed identity..."
MI_PRINCIPAL_ID=$(az identity show \
  --name "${MI_NAME}" \
  --resource-group "${MI_RESOURCE_GROUP}" \
  --query 'principalId' -o tsv)

if [ -z "${MI_PRINCIPAL_ID}" ]; then
  echo "ERROR: Could not find managed identity '${MI_NAME}' in resource group '${MI_RESOURCE_GROUP}'"
  exit 1
fi
echo "Managed Identity Principal ID: ${MI_PRINCIPAL_ID}"

# Get the Microsoft Graph service principal
# Microsoft Graph well-known appId: 00000003-0000-0000-c000-000000000000
echo "Looking up Microsoft Graph service principal..."
GRAPH_SP_ID=$(az ad sp show --id "00000003-0000-0000-c000-000000000000" --query 'id' -o tsv)
echo "Microsoft Graph SP Object ID: ${GRAPH_SP_ID}"

# Mail.Send application permission role ID
# This is a well-known GUID for the Mail.Send app role in Microsoft Graph
MAIL_SEND_ROLE_ID=$(az ad sp show --id "00000003-0000-0000-c000-000000000000" \
  --query "appRoles[?value=='Mail.Send'].id | [0]" -o tsv)

if [ -z "${MAIL_SEND_ROLE_ID}" ]; then
  echo "ERROR: Could not find Mail.Send app role in Microsoft Graph"
  exit 1
fi
echo "Mail.Send Role ID: ${MAIL_SEND_ROLE_ID}"

# Check if assignment already exists
echo "Checking for existing role assignment..."
EXISTING=$(az rest --method GET \
  --uri "https://graph.microsoft.com/v1.0/servicePrincipals/${MI_PRINCIPAL_ID}/appRoleAssignments" \
  --query "value[?appRoleId=='${MAIL_SEND_ROLE_ID}' && resourceId=='${GRAPH_SP_ID}'].id | [0]" \
  -o tsv 2>/dev/null || true)

if [ -n "${EXISTING}" ] && [ "${EXISTING}" != "None" ]; then
  echo "Mail.Send permission is already assigned to the managed identity."
else
  echo "Assigning Mail.Send permission to managed identity..."
  az rest --method POST \
    --uri "https://graph.microsoft.com/v1.0/servicePrincipals/${MI_PRINCIPAL_ID}/appRoleAssignments" \
    --headers "Content-Type=application/json" \
    --body "{
      \"principalId\": \"${MI_PRINCIPAL_ID}\",
      \"resourceId\": \"${GRAPH_SP_ID}\",
      \"appRoleId\": \"${MAIL_SEND_ROLE_ID}\"
    }"
  echo "Mail.Send permission assigned successfully."
fi

echo ""
echo "=== Graph API Permission Setup Complete ==="
echo "Managed Identity: ${MI_NAME}"
echo "Principal ID:     ${MI_PRINCIPAL_ID}"
echo "Permission:       Mail.Send (Application)"
echo ""
echo "Note: This grants the managed identity permission to send emails"
echo "as any user in the tenant. The backend uses this to send ticket"
echo "confirmation emails via Microsoft Graph API."
