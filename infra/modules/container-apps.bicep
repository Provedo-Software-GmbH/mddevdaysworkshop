@description('The name of the existing Container Apps Environment')
param environmentName string

@description('The location for the container apps (must match the Container Apps Environment location)')
param containerAppsLocation string = 'westeurope'

@description('The name of the backend Container App')
param backendAppName string

@description('The name of the frontend Container App')
param frontendAppName string

@description('The ACR login server')
param acrLoginServer string

@description('The backend image name (without tag)')
param backendImageName string = 'devconf-ticketing-api'

@description('The frontend image name (without tag)')
param frontendImageName string = 'devconf-ticketing-frontend'

@description('The image tag')
param imageTag string = 'latest'

@description('The resource ID of the user-assigned managed identity')
param managedIdentityId string

@description('The client ID of the user-assigned managed identity')
param managedIdentityClientId string

@description('The Key Vault URL')
param keyVaultUrl string

@description('The Cosmos DB account endpoint')
param cosmosDbEndpoint string

@description('The Cosmos DB database name')
param cosmosDbDatabaseName string = 'devconf-ticketing'

@description('The Entra ID tenant ID for authentication')
param entraIdTenantId string

@description('The Entra ID client ID for the backend API app registration')
param entraIdClientId string

@description('The Entra ID audience for the backend API')
param entraIdAudience string

@description('The custom domain name for the backend app (e.g., tickets.v2.api.devconf.nrw)')
param backendCustomDomain string = ''

@description('The custom domain name for the frontend app (e.g., tickets.v2.devconf.nrw)')
param frontendCustomDomain string = ''

@description('The name of the certificate in the Container Apps Environment for the custom domains')
param customDomainCertificateName string = ''

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: environmentName
}

resource certificate 'Microsoft.App/managedEnvironments/certificates@2024-03-01' existing = if (!empty(customDomainCertificateName)) {
  parent: environment
  name: customDomainCertificateName
}

resource backendApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: backendAppName
  location: containerAppsLocation
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 5000
        transport: 'http'
        allowInsecure: false
        customDomains: !empty(backendCustomDomain) && !empty(customDomainCertificateName) ? [
          {
            name: backendCustomDomain
            certificateId: certificate.id
            bindingType: 'SniEnabled'
          }
        ] : []
      }
      registries: [
        {
          server: acrLoginServer
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'backend'
          image: '${acrLoginServer}/${backendImageName}:${imageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: managedIdentityClientId
            }
            {
              name: 'KeyVault__Url'
              value: keyVaultUrl
            }
            {
              name: 'CosmosDb__AccountEndpoint'
              value: cosmosDbEndpoint
            }
            {
              name: 'CosmosDb__DatabaseName'
              value: cosmosDbDatabaseName
            }
            {
              name: 'AzureAd__Instance'
              value: az.environment().authentication.loginEndpoint
            }
            {
              name: 'AzureAd__TenantId'
              value: entraIdTenantId
            }
            {
              name: 'AzureAd__ClientId'
              value: entraIdClientId
            }
            {
              name: 'AzureAd__Audience'
              value: entraIdAudience
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: !empty(frontendCustomDomain) ? 'https://${frontendCustomDomain}' : 'https://${frontendAppName}.${environment.properties.defaultDomain}'
            }
            {
              name: 'Cors__AllowedOrigins__1'
              value: 'https://${frontendAppName}.${environment.properties.defaultDomain}'
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 5000
              }
              periodSeconds: 30
              initialDelaySeconds: 10
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 5000
              }
              periodSeconds: 10
              initialDelaySeconds: 5
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 5
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}

resource frontendApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: frontendAppName
  location: containerAppsLocation
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        transport: 'http'
        allowInsecure: false
        customDomains: !empty(frontendCustomDomain) && !empty(customDomainCertificateName) ? [
          {
            name: frontendCustomDomain
            certificateId: certificate.id
            bindingType: 'SniEnabled'
          }
        ] : []
      }
      registries: [
        {
          server: acrLoginServer
          identity: managedIdentityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'frontend'
          image: '${acrLoginServer}/${frontendImageName}:${imageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
}

@description('The backend app FQDN')
output backendFqdn string = backendApp.properties.configuration.ingress.fqdn

@description('The frontend app FQDN')
output frontendFqdn string = frontendApp.properties.configuration.ingress.fqdn

@description('The backend app name')
output backendName string = backendApp.name

@description('The frontend app name')
output frontendName string = frontendApp.name
