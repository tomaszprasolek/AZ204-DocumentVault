targetScope = 'subscription'

param parLocation string = 'northeurope'

resource resGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: 'rg-DocumentVault-ne'
  location: parLocation
}

