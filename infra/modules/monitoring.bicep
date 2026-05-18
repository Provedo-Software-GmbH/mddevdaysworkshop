@description('The name of the Application Insights resource')
param appInsightsName string

@description('The location for the resources')
param location string = resourceGroup().location

@description('The existing Log Analytics workspace name')
param logAnalyticsWorkspaceName string

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logAnalyticsWorkspaceName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('The Application Insights connection string')
output connectionString string = appInsights.properties.ConnectionString

@description('The Application Insights name')
output name string = appInsights.name
