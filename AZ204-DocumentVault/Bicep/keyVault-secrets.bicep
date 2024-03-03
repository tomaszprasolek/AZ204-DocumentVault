// Parameters
param parKeyVaultName string
param parStorageAccountName string
param parCosmosDbName string

// Key vault
resource resKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing ={
  name: parKeyVaultName
}

// Storage account
resource resStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: parStorageAccountName
}

// Cosmos Db
resource resCosmosDb 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' existing = {
  name: parCosmosDbName
}

// ------------------------------------
// Create a secrets outside of key vault definition
// ------------------------------------

// Storage account key
resource resSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: resKeyVault
  name: 'StorageAccountKey'
  properties: {
    value: resStorageAccount.listKeys().keys[0].value
  }
}

// Comsos DB primary master key
resource resSecretCosmosDbKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: resKeyVault
  name: 'CosmosDbKey'
  properties: {
    value: resCosmosDb.listKeys().primaryMasterKey
  }
}

// Function app URL code
resource resSecretFunctionAppUrlCode 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: resKeyVault
  name: 'test'
  properties: {
    value: resCosmosDb.listKeys().primaryMasterKey
  }
}
