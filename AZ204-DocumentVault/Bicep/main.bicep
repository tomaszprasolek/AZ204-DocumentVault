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

// Key vault
module modKeyVault 'keyVault.bicep' = {
  name: 'modKeyVault'
  params: {
    parLocation: parLocation
    parPrincipalId: parPrincipalId
    parAppServiceObjectId: modAppService.outputs.outAppServiceObjectId
    parStorageAccountName: modStorageAccount.outputs.storageAccountName
  }
}
