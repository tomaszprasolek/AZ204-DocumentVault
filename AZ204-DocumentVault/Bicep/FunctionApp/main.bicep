targetScope = 'subscription'

param parLocation string = 'northeurope'
param parStorageAccountResourceGroup string
param parStorageAccountName string
param parStorageAccountContainerName string
param parKeyVaultUri string

resource newRG 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-DocumentVault-FunctionApp-ne'
  location: parLocation
}

// Function App
module modFunctionApp 'functionApp.bicep' = {
  name: 'functionApp'
  scope: resourceGroup(newRG.name)
  params: {
    parLocation: parLocation
    parStorageAccountResourceGroup: parStorageAccountResourceGroup
    parKeyVaultUri: parKeyVaultUri
    parStorageAccountContainerName: parStorageAccountContainerName
    parStorageAccountName: parStorageAccountName
  }
}
