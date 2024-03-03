param parLocation string = resourceGroup().location
param parPrincipalId string

// Storage account
module modStorageAccount 'storageAccount.bicep' = {
  name: 'modStorageAccount'
  params: {
    parLocation: parLocation
    parPrincipalId: parPrincipalId
  }
}

// App service
module modAppService 'appService.bicep' = {
  name: 'modAppService'
  params: {
    parLocation: parLocation
  }
}

// Cosmos DB
module modCosmosDb 'cosmosDb.bicep' = {
  name: 'modCosmosDb'
  params: {
    parLocation: parLocation
  }
}

// Key vault
module modKeyVault 'keyVault.bicep' = {
  name: 'modKeyVault'
  params: {
    parLocation: parLocation
    parPrincipalId: parPrincipalId
  }
}

// Function app
module modFunctionApp 'FunctionApp/main.bicep' = {
  scope: subscription()
  name: 'functionApp'
  params: {
    parLocation: parLocation
    parStorageAccountResourceGroup: resourceGroup().name
    parKeyVaultUri: modKeyVault.outputs.keyVaultUri
    parStorageAccountContainerName: modStorageAccount.outputs.storageAccountContainerName
    parStorageAccountName: modStorageAccount.outputs.storageAccountName
  }
}

// Key vault - add access policies
module modAccessPolicies 'keyVault-accessPolicies.bicep' = {
  name: 'accessPolicies'
  params: {
    parAppServiceObjectId: modAppService.outputs.outAppServiceObjectId
    parFunctionAppObjectId: modFunctionApp.outputs.outFunctionAppObjectId
    parKeyVaultName: modKeyVault.outputs.keyVaultName
  }
}

// Key vault - add secrets
module modSecrets 'keyVault-secrets.bicep' = {
  name: 'modSecrets'
  params: {
    parCosmosDbName: modCosmosDb.outputs.cosmosDbName
    parKeyVaultName: modKeyVault.outputs.keyVaultName
    parStorageAccountName: modStorageAccount.outputs.storageAccountName
  }
}
