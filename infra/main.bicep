@description('Environment name (dev, staging, prod)')
param environmentName string = 'prod'

@description('Azure region for Cosmos DB and Application Insights')
param location string = resourceGroup().location

@description('Azure region for Static Web Apps (must be an SWA-supported region)')
param swaLocation string = 'eastus2'

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

// ── Azure Static Web App ───────────────────────────────────────────────────
resource staticWebApp 'Microsoft.Web/staticSites@2023-01-01' = {
  name: '${prefix}-swa'
  location: swaLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// ── Static Web App — API environment variables ─────────────────────────────
resource swaAppSettings 'Microsoft.Web/staticSites/config@2023-01-01' = {
  parent: staticWebApp
  name: 'appsettings'
  properties: {
    CosmosDBConnectionString: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
    CosmosDBDatabaseName: 'study-tracker'
    CosmosDBContainerName: 'sessions'
  }
}

// ── Outputs ────────────────────────────────────────────────────────────────
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'

@description('Add this as the AZURE_STATIC_WEB_APPS_API_TOKEN GitHub Actions secret')
@secure()
output deploymentToken string = staticWebApp.listSecrets().properties.apiKey

output cosmosAccountName string = cosmosAccount.name
