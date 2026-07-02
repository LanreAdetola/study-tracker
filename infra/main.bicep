@description('Environment name (dev, staging, prod)')
param environmentName string = 'prod'

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('App Service Plan SKU (Linux)')
param appServicePlanSku string = 'B1'

@description('Container image to deploy, e.g. ghcr.io/<owner>/study-tracker:latest')
param containerImage string = 'ghcr.io/placeholder/study-tracker:latest'

@description('GitHub OAuth app client ID (App Service Authentication)')
param githubClientId string = ''

@description('GitHub OAuth app client secret (App Service Authentication)')
@secure()
param githubClientSecret string = ''

@description('Microsoft Entra app registration client ID (App Service Authentication)')
param aadClientId string = ''

@description('Microsoft Entra app registration client secret (App Service Authentication)')
@secure()
param aadClientSecret string = ''

@description('ghcr.io registry username, required only if the container image is private')
param registryUsername string = ''

@description('ghcr.io registry password/PAT, required only if the container image is private')
@secure()
param registryPassword string = ''

var appName = 'study-tracker'
var prefix = '${appName}-${environmentName}'

// ── Cosmos DB Account (Serverless) ─────────────────────────────────────────
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: '${prefix}-cosmos'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      { name: 'EnableServerless' }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    enableAutomaticFailover: false
  }
}

// ── Cosmos DB Database ─────────────────────────────────────────────────────
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  parent: cosmosAccount
  name: 'study-tracker'
  properties: {
    resource: { id: 'study-tracker' }
  }
}

// ── Cosmos DB Containers ───────────────────────────────────────────────────
resource sessionsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDatabase
  name: 'sessions'
  properties: {
    resource: {
      id: 'sessions'
      partitionKey: { paths: ['/userId'], kind: 'Hash' }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [{ path: '/*' }]
        excludedPaths: [{ path: '/"_etag"/?' }]
      }
    }
  }
}

resource goalsContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDatabase
  name: 'goals'
  properties: {
    resource: {
      id: 'goals'
      partitionKey: { paths: ['/userId'], kind: 'Hash' }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [{ path: '/*' }]
        excludedPaths: [{ path: '/"_etag"/?' }]
      }
    }
  }
}

resource usersContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDatabase
  name: 'users'
  properties: {
    resource: {
      id: 'users'
      partitionKey: { paths: ['/userId'], kind: 'Hash' }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [{ path: '/*' }]
        excludedPaths: [{ path: '/"_etag"/?' }]
      }
    }
  }
}

// ── App Service Plan (Linux, container-based) ──────────────────────────────
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${prefix}-plan'
  location: location
  kind: 'linux'
  sku: {
    name: appServicePlanSku
  }
  properties: {
    reserved: true
  }
}

var hasRegistryCredentials = !empty(registryUsername) && !empty(registryPassword)

// ── App Service (Web App for Containers) ───────────────────────────────────
resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${prefix}-app'
  location: location
  kind: 'app,linux,container'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${containerImage}'
    }
  }
}

// ── App Service — application settings (Cosmos config, OAuth secrets, registry creds) ──
resource webAppSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'appsettings'
  properties: union({
    WEBSITES_PORT: '8080'
    CosmosDBConnectionString: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
    CosmosDBDatabaseName: 'study-tracker'
    GITHUB_CLIENT_SECRET: githubClientSecret
    AAD_CLIENT_SECRET: aadClientSecret
  }, hasRegistryCredentials ? {
    DOCKER_REGISTRY_SERVER_URL: 'https://ghcr.io'
    DOCKER_REGISTRY_SERVER_USERNAME: registryUsername
    DOCKER_REGISTRY_SERVER_PASSWORD: registryPassword
  } : {})
}

// ── App Service Authentication (Easy Auth v2) ──────────────────────────────
resource authSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'authsettingsV2'
  properties: {
    platform: {
      enabled: true
    }
    globalValidation: {
      unauthenticatedClientAction: 'AllowAnonymous'
    }
    identityProviders: {
      gitHub: {
        enabled: !empty(githubClientId)
        registration: {
          clientId: githubClientId
          clientSecretSettingName: 'GITHUB_CLIENT_SECRET'
        }
      }
      azureActiveDirectory: {
        enabled: !empty(aadClientId)
        registration: {
          clientId: aadClientId
          clientSecretSettingName: 'AAD_CLIENT_SECRET'
        }
      }
    }
  }
  dependsOn: [
    webAppSettings
  ]
}

// ── Outputs ────────────────────────────────────────────────────────────────
output appUrl string = 'https://${webApp.properties.defaultHostName}'
output cosmosAccountName string = cosmosAccount.name
output appServiceName string = webApp.name
