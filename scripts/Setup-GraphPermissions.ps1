# ──────────────────────────────────────────────────────────────────────
# Setup-GraphPermissions.ps1
#
# Assigns Microsoft Graph application permissions (Mail.Send) directly
# to a user-assigned managed identity's service principal.
#
# This CANNOT be done via Bicep — it requires Microsoft Graph API calls
# to create appRoleAssignments on the managed identity's service principal.
#
# Prerequisites:
#   - Azure CLI installed and logged in with sufficient permissions
#   - The managed identity must already exist
#
# Usage:
#   .\scripts\Setup-GraphPermissions.ps1 [-ManagedIdentityName <name>] [-ResourceGroup <rg>]
# ──────────────────────────────────────────────────────────────────────

param(
    [string]$ManagedIdentityName = 'nrwdevconf_mi',
    [string]$ResourceGroup = 'nrwdevconf'
)

$ErrorActionPreference = 'Stop'

Write-Host "=== Setting up Graph API permissions for Managed Identity: $ManagedIdentityName ==="
Write-Host 'Note: Pass -ManagedIdentityName and -ResourceGroup if different from defaults.'

# Get the managed identity's principal ID (object ID of its service principal)
Write-Host 'Looking up managed identity...'
$MiPrincipalId = az identity show `
    --name $ManagedIdentityName `
    --resource-group $ResourceGroup `
    --query 'principalId' -o tsv

if (-not $MiPrincipalId) {
    Write-Error "Could not find managed identity '$ManagedIdentityName' in resource group '$ResourceGroup'"
    exit 1
}
Write-Host "Managed Identity Principal ID: $MiPrincipalId"

# Get the Microsoft Graph service principal
# Microsoft Graph well-known appId: 00000003-0000-0000-c000-000000000000
Write-Host 'Looking up Microsoft Graph service principal...'
$GraphSpId = az ad sp show --id '00000003-0000-0000-c000-000000000000' --query 'id' -o tsv
Write-Host "Microsoft Graph SP Object ID: $GraphSpId"

# Mail.Send application permission role ID
$MailSendRoleId = az ad sp show --id '00000003-0000-0000-c000-000000000000' `
    --query "appRoles[?value=='Mail.Send'].id | [0]" -o tsv

if (-not $MailSendRoleId) {
    Write-Error 'Could not find Mail.Send app role in Microsoft Graph'
    exit 1
}
Write-Host "Mail.Send Role ID: $MailSendRoleId"

# Check if assignment already exists
Write-Host 'Checking for existing role assignment...'
$Existing = az rest --method GET `
    --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$MiPrincipalId/appRoleAssignments" `
    --query "value[?appRoleId=='$MailSendRoleId' && resourceId=='$GraphSpId'].id | [0]" `
    -o tsv 2>&1 | Where-Object { $_ -is [string] }

if ($Existing -and $Existing -ne 'None') {
    Write-Host 'Mail.Send permission is already assigned to the managed identity.'
}
else {
    Write-Host 'Assigning Mail.Send permission to managed identity...'
    $Body = @{
        principalId = $MiPrincipalId
        resourceId  = $GraphSpId
        appRoleId   = $MailSendRoleId
    } | ConvertTo-Json -Compress

    az rest --method POST `
        --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$MiPrincipalId/appRoleAssignments" `
        --headers 'Content-Type=application/json' `
        --body $Body

    Write-Host 'Mail.Send permission assigned successfully.'
}

Write-Host ''
Write-Host '=== Graph API Permission Setup Complete ==='
Write-Host "Managed Identity: $ManagedIdentityName"
Write-Host "Principal ID:     $MiPrincipalId"
Write-Host 'Permission:       Mail.Send (Application)'
Write-Host ''
Write-Host 'Note: This grants the managed identity permission to send emails'
Write-Host 'as any user in the tenant. The backend uses this to send ticket'
Write-Host 'confirmation emails via Microsoft Graph API.'
