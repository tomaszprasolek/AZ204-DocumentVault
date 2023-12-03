param parLocation string = resourceGroup().location

resource resStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'documentvaultaz204'
  location: parLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'BlobStorage'
}
