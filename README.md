# AZ204-DocumentVault
Azure Document Vault with Expiry &amp; CDN Integration (Develop for Azure storage)

## Azure resources

- Azure App Service - web application to upload documents to Azure
- Azure Storage - to store the uploaded documents
- Azure Key Vault - to store the secrets

## TODO
- [X] Bicep - create Cosmos DB using main script
- [X] Bicep - add Cosmos DB key to Key Vault
- [X] Setup CI/CD in Github Actions
- [ ] Remove publishToAzure.run - use `git filter-repo` >> https://gist.github.com/tomaszprasolek/a1d66512bf30afd5019df6b20a2255ab

## Commands

Deploy Bicep script when create resource group in it:

```
az deployment sub create --location northeurope --template-file .\AZ204-DocumentVault\Bicep\main.bicep
```

Create all needed resources:
```
az deployment group create --resource-group rg-DocumentVault-ne --template-file .\AZ204-DocumentVault\Bicep\main.bicep --parameter parPrincipalId='azure-user-object-id'
```
## Links 

### Azure Cosmos Db

- https://stackoverflow.com/questions/63243857/what-does-upsertitemasync-do-in-the-net-cosmos-db-client
- https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update
- https://learn.microsoft.com/en-us/azure/cosmos-db/partial-document-update-getting-started?tabs=dotnet
