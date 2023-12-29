# AZ204-DocumentVault
Azure Document Vault with Expiry &amp; CDN Integration (Develop for Azure storage)

## Azure resources

- Azure App Service - web application to upload documents to Azure
- Azure Storage - to store the uploaded documents
- Azure Key Vault - to store the secrets

## TODO
- Bicep - create Cosmos DB using main script
- Bicep - add Cosmos DB key to Key Vault
- Setup CI/CD in Github Actions

## Commands

Deploy Bicep script when create resource group in it:

```
az deployment sub create --location northeurope --template-file .\AZ204-DocumentVault\Bicep\main.bicep
```

Create all needed resources:
```
az deployment group create --resource-group rg-DocumentVault-ne --template-file .\AZ204-DocumentVault\Bicep\main.bicep --parameter parPrincipalId='azure-user-object-id'
```
