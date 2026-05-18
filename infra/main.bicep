targetScope = 'resourceGroup'

// ──────────────────────────────────────────────
// Parameters
// ──────────────────────────────────────────────

@description('The environment name (e.g., prod, dev)')
param environmentName string

@description('The location for new resources')
param location string = resourceGroup().location

@description('The name of the existing Container Apps Environment')
param containerAppsEnvironmentName string

@description('The name of the existing user-assigned managed identity')
param managedIdentityName string

@description('The name of the existing Key Vault')
param keyVaultName string

@description('The name of the existing Log Analytics workspace')
param logAnalyticsWorkspaceName string

@description('The name of the ACR (existing, in a different resource group)')
param acrName string

@description('The resource group of the ACR')
param acrResourceGroup string

@description('The ACR login server')
param acrLoginServer string

@description('The Cosmos DB account name')
param cosmosDbAccountName string

@description('The Application Insights resource name')
param appInsightsName string

@description('The backend Container App name')
param backendAppName string

@description('The frontend Container App name')
param frontendAppName string

@description('The image tag for container deployments')
param imageTag string = 'latest'

@description('The Entra ID tenant ID')
param entraIdTenantId string

@description('The Entra ID client ID for the backend API app registration')
param entraIdClientId string

@description('The Entra ID audience for the backend API')
param entraIdAudience string

// ──────────────────────────────────────────────
// Existing resources
// ──────────────────────────────────────────────

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: managedIdentityName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// ──────────────────────────────────────────────
// Modules
// ──────────────────────────────────────────────

module containerAppsEnvironment 'modules/container-apps-environment.bicep' = {
  name: 'container-apps-env-${environmentName}'
  params: {
    environmentName: containerAppsEnvironmentName
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
  }
}

module cosmosDb 'modules/cosmos-db.bicep' = {
  name: 'cosmos-db-${environmentName}'
  params: {
    accountName: cosmosDbAccountName
    location: location
    managedIdentityPrincipalId: managedIdentity.properties.principalId
  }
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring-${environmentName}'
  params: {
    appInsightsName: appInsightsName
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
  }
}

module keyVaultSecrets 'modules/key-vault-secrets.bicep' = {
  name: 'key-vault-secrets-${environmentName}'
  params: {
    keyVaultName: keyVaultName
    managedIdentityPrincipalId: managedIdentity.properties.principalId
    cosmosDbEndpoint: cosmosDb.outputs.accountEndpoint
    appInsightsConnectionString: monitoring.outputs.connectionString
  }
}

module acrRoleAssignment 'modules/acr-role-assignment.bicep' = {
  name: 'acr-role-assignment-${environmentName}'
  scope: resourceGroup(acrResourceGroup)
  params: {
    acrName: acrName
    managedIdentityPrincipalId: managedIdentity.properties.principalId
  }
}

module containerApps 'modules/container-apps.bicep' = {
  name: 'container-apps-${environmentName}'
  params: {
    environmentName: containerAppsEnvironmentName
    location: location
    backendAppName: backendAppName
    frontendAppName: frontendAppName
    acrLoginServer: acrLoginServer
    imageTag: imageTag
    managedIdentityId: managedIdentity.id
    managedIdentityClientId: managedIdentity.properties.clientId
    keyVaultUrl: keyVault.properties.vaultUri
    cosmosDbEndpoint: cosmosDb.outputs.accountEndpoint
    entraIdTenantId: entraIdTenantId
    entraIdClientId: entraIdClientId
    entraIdAudience: entraIdAudience
  }
  dependsOn: [
    containerAppsEnvironment
    keyVaultSecrets
    acrRoleAssignment
  ]
}

// ──────────────────────────────────────────────
// Outputs
// ──────────────────────────────────────────────

@description('The backend Container App FQDN')
output backendFqdn string = containerApps.outputs.backendFqdn

@description('The frontend Container App FQDN')
output frontendFqdn string = containerApps.outputs.frontendFqdn

@description('The Cosmos DB account endpoint')
output cosmosDbEndpoint string = cosmosDb.outputs.accountEndpoint
