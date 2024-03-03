// Parameters
param parLocation string = resourceGroup().location

@description('The Azure user object-id')
param parPrincipalId string

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

output keyVaultName string = resKeyVault.name
output keyVaultUri string = resKeyVault.properties.vaultUri
