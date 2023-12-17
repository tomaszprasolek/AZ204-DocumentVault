// Parameters
param parLocation string = resourceGroup().location

// Resources
resource resDbAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: 'cosdb-documentvault-ne'
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
  parent: resDbAccount
  name: 'DocumentsVault'
  properties: {
    resource: {
      id: 'DocumentsVault'
    }
  }
}

resource resContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: resDatabase
  name: 'Documents'
  properties: {
    resource: {
      id: 'Documents'
      partitionKey: {
        
      }
    }
  }
}
