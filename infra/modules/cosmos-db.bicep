@description('The name of the Cosmos DB account')
param accountName string

@description('The location for the Cosmos DB account')
param location string = resourceGroup().location

@description('The name of the database')
param databaseName string = 'devconf-ticketing'

@description('The principal ID of the managed identity to grant data contributor role')
param managedIdentityPrincipalId string

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-12-01-preview' = {
  name: accountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    backupPolicy: {
      type: 'Continuous'
      continuousModeProperties: {
        tier: 'Continuous7Days'
      }
    }
    disableLocalAuth: true
  }
}

resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-12-01-preview' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
  }
}

var containers = [
  { name: 'events', partitionKey: '/id' }
  { name: 'ticket-types', partitionKey: '/eventId' }
  { name: 'tax-rates', partitionKey: '/countryCode' }
  { name: 'orders', partitionKey: '/eventId' }
  { name: 'customers', partitionKey: '/id' }
  { name: 'vouchers', partitionKey: '/eventId' }
  { name: 'checkin-lists', partitionKey: '/eventId' }
  { name: 'checkin-records', partitionKey: '/eventId' }
  { name: 'invoices', partitionKey: '/orderId' }
]

resource cosmosContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-12-01-preview' = [
  for container in containers: {
    parent: database
    name: container.name
    properties: {
      resource: {
        id: container.name
        partitionKey: {
          paths: [container.partitionKey]
          kind: 'Hash'
          version: 2
        }
      }
    }
  }
]

// Cosmos DB Built-in Data Contributor role
var cosmosDataContributorRoleId = '00000000-0000-0000-0000-000000000002'

resource cosmosRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-12-01-preview' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, managedIdentityPrincipalId, cosmosDataContributorRoleId)
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${cosmosDataContributorRoleId}'
    principalId: managedIdentityPrincipalId
    scope: cosmosAccount.id
  }
}

@description('The Cosmos DB account endpoint')
output accountEndpoint string = cosmosAccount.properties.documentEndpoint

@description('The Cosmos DB account name')
output name string = cosmosAccount.name
