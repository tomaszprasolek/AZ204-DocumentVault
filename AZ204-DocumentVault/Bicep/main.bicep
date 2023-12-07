param parLocation string = resourceGroup().location
param parPrincipalId string

resource resStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'documentvaultaz204'
  location: parLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'BlobStorage'
  properties:{
    accessTier: 'Hot'
  }
}

resource resBlobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: resStorageAccount
  name: 'default'
}

resource resBlobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: resBlobService
  name: 'default'
}

resource resKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'documentsKeyVault'
  location: parLocation
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
  }
}

var varRoleId = '00482a5a-887f-4fb3-b363-3b7fe8e74483' // Key vault administrator

resource resRegistryRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().subscriptionId, resourceGroup().name, resKeyVault.name, varRoleId, parPrincipalId)
  scope: resKeyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', varRoleId)
    principalId: parPrincipalId
    principalType: 'User'
  }
}
