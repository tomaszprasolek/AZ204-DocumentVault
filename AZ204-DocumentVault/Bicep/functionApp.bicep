// Parameters
param parLocation string = resourceGroup().location

// ------------------------------------------------
// Create storage account for function app prereq
// ------------------------------------------------
resource resStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: 'fastorageacct'
  location: parLocation
  kind: 'StorageV2' // Storage
  sku: {
    name: 'Standard_LRS'
  }
}

var azStorageAccountPrimaryAccessKey = resStorageAccount.listKeys().keys[0].value

// ------------------------------------------------
// Create Application Insights
// ------------------------------------------------
resource resAppInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'functionApp-ai'
  location: parLocation
  kind: 'web'
  properties:{
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

var azAppInsightsInstrumentationKey = resAppInsights.properties.InstrumentationKey

// ------------------------------------------------
// App service plan
// ------------------------------------------------
resource resAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: 'functionApp-servicePlan'
  location: parLocation
  properties:{
    reserved: true
  }
  sku:{
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
    family: 'Y'
    capacity: 0
  }
  kind: 'linux'
}

// ------------------------------------------------
// Azure function
// ------------------------------------------------
resource azFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: 'functionApp-app'
  kind: 'functionapp'
  location: parLocation
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: resAppServicePlan.id
    clientAffinityEnabled: false
    reserved: true
    siteConfig: {
      alwaysOn: false
      linuxFxVersion: 'DOTNET-ISOLATED|6.0'
      appSettings: [
        {
          name: 'AzureWebJobsDashboard'
          value: 'DefaultEndpointsProtocol=https;AccountName=${resStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${azStorageAccountPrimaryAccessKey}'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${resStorageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${azStorageAccountPrimaryAccessKey}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: azAppInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${azAppInsightsInstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'false'
        }
        {
          name: 'WEBSITE_ENABLE_SYNC_UPDATE_SITE'
          value: 'true'
        }
        {
          name: 'StorageAccountName'
          value: 'documentvaultaz204'
        }
        {
          name: 'ContainerName'
          value: 'documents'
        }
        {
          name: 'KeyVaultUri'
          value: 'https://documentskeyvault.vault.azure.net/'
        }
      ]
    }
  }
}
