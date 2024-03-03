// Parameters
param parLocation string = resourceGroup().location

@description('The Azure user object-id')
param parPrincipalId string

param parAppServiceObjectId string
param parStorageAccountName string
param parCosmosDbName string

// Variables
var varTenantId = subscription().tenantId

// Resources
resource resKeyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'documentsKeyVault'
  location: parLocation
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: varTenantId
    accessPolicies: [
      {
        objectId: parAppServiceObjectId
        permissions: {
          secrets: [
            'get'
          ]
        }
        tenantId: varTenantId
      }
    ]
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

// -------------------
// Add secrets
// -------------------

// Storage account
resource resStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: parStorageAccountName
}

resource resSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: resKeyVault
  name: 'StorageAccountKey'
  properties: {
    value: resStorageAccount.listKeys().keys[0].value
  }
}

// Cosmos Db
resource resCosmosDb 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' existing = {
  name: parCosmosDbName
}

resource resSecretCosmosDbKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: resKeyVault
  name: 'CosmosDbKey'
  properties: {
    value: resCosmosDb.listKeys().primaryMasterKey
  }
}

output keyVaultUri string = resKeyVault.properties.vaultUri
