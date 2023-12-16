param parLocation string = resourceGroup().location
param parPrincipalId string

// Storage account
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
  name: 'documents'
}

// Role assignment
var varRoleStorageAccountContributor = '17d1049b-9a84-46fb-8f53-869881c3d3ab' // Storage account contributor

resource resStorageAccountContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(subscription().subscriptionId, resourceGroup().name, resBlobContainer.name, varRoleStorageAccountContributor, parPrincipalId)
  scope: resBlobContainer
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', varRoleStorageAccountContributor)
    principalId: parPrincipalId
    principalType: 'User'
  }
}

output storageAccountName string = resStorageAccount.name
