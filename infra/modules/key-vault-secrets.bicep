@description('The name of the existing Key Vault')
param keyVaultName string

@description('The principal ID of the managed identity to grant Key Vault Secrets User role')
param managedIdentityPrincipalId string

@description('The Cosmos DB account endpoint')
param cosmosDbEndpoint string

@description('The Application Insights connection string')
param appInsightsConnectionString string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Key Vault Secrets User role
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource kvRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, managedIdentityPrincipalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource cosmosEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'CosmosDb--AccountEndpoint'
  properties: {
    value: cosmosDbEndpoint
  }
}

resource appInsightsSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ApplicationInsights--ConnectionString'
  properties: {
    value: appInsightsConnectionString
  }
}
