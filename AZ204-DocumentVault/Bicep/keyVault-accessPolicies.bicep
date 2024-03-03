// Parameters
param parKeyVaultName string
param parAppServiceObjectId string
param parFunctionAppObjectId string

// Variables
var varTenantId = subscription().tenantId

resource resAccessPolicies 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  name: '${parKeyVaultName}/add'
  properties: {
    accessPolicies: [
      {
        objectId: parAppServiceObjectId
        tenantId: varTenantId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
      {
        objectId: parFunctionAppObjectId
        tenantId: varTenantId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
    ]
  }
}
