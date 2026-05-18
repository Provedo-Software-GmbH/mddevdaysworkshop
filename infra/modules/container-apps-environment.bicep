@description('The name of the Container Apps Environment')
param environmentName string

@description('The location for the resource')
param location string = resourceGroup().location

@description('The existing Log Analytics workspace name')
param logAnalyticsWorkspaceName string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logAnalyticsWorkspaceName
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    zoneRedundant: false
  }
}

@description('The Container Apps Environment ID')
output id string = environment.id

@description('The Container Apps Environment name')
output name string = environment.name
