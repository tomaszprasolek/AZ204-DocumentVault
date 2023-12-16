// Parameters
param parLocation string = resourceGroup().location

// Resources
resource resAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: 'appServicePlan'
  location: parLocation
  properties:{
    reserved: true
  }
  sku:{
    name: 'F1'
  }
  kind: 'linux'
}

resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: 'webApp-DocumentVault-ne'
  location: parLocation
  properties: {
    serverFarmId: resAppServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|7.0'
    }
  }
  identity: {
    type:'SystemAssigned'
  }
}

output outAppServiceObjectId string = appService.identity.principalId
