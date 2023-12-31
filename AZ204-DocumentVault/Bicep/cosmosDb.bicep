// Parameters
param parLocation string = resourceGroup().location

// Resources
resource resDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: 'cosdb-documentvault-ne'
  location: parLocation
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: parLocation
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}

resource resDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  name: 'DocumentsVault'
  parent: resDbAccount
  location: parLocation
  properties: {
    resource: {
      id: 'DocumentsVault'
    }
  }
}

resource resContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  name: 'Documents'
  parent: resDatabase
  location: parLocation
  properties: {
    resource: {
      id: 'Documents'
      partitionKey: {
        paths:[
          '/id'
        ]
        kind: 'Hash'
      }
    }
  }
}

output cosmosDbName string = resDbAccount.name
