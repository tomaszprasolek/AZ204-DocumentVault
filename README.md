# AZ204-DocumentVault
Azure Document Vault with Expiry &amp; CDN Integration (Develop for Azure storage)

## Azure resources

- Azure App Service - web application to upload documents to Azure
- Azure Storage - store the uploaded documents
- Azure Key Vault - store the secrets
- Cosmos Db - store documents data and metadata
- App registraion - to be able to log in using Azure credentials

## TODO
- [X] Bicep - create Cosmos DB using main script
- [X] Bicep - add Cosmos DB key to Key Vault
- [X] Setup CI/CD in Github Actions
- [X] Filter data by logged in user ID
- [X] Azure Function - use it to generate download link to file
- [ ] Azure Function
  - [ ] Create Azure Function using Bicep
    - [ ] Add Function to has access to KeyVault
    - [ ] Use the same service plan as App Service has
  - [ ] Create Github Action to deploy the function to Azure
  - [ ] Add application settings - I don't know where?? In Bicep or Github Actions?
    - [ ] StorageAccountName
    - [ ] ContainerName
    - [ ] KeyVaultUri
- [ ] Remove publishToAzure.run - use `git filter-repo` >> https://gist.github.com/tomaszprasolek/a1d66512bf30afd5019df6b20a2255ab

## How to set up the CI/CD on Github and environment on Azure

- Create resource group when all other Azure resource will be placed
- Get your principal identifier from Azure, it is needed for the next script. You can find it: `Users >> your user >> Object Id`.
- Run Azure Bicep script: `main.bicep` (AZ204-DocumentVault/Bicep/main.bicep) and pass `Object Id` as parameter
  - Command to run script: `az deployment group create --resource-group rg-DocumentVault-ne --template-file .\AZ204-DocumentVault\Bicep\main.bicep --parameter parPrincipalId='azure-user-object-id'`. **Remember changing the resource group name and principal id.**
  - The User `Object Id` is needed for... **TODO: add what for is ObjectId needed** 
- After create the resource in Azure, download publish profile from Azure:
  - `Portal Azure >> Resource group >> App service >> Download publish profile`
- Put it in repository secret on Github. Secret name: `AZURE_WEBAPP_PUBLISH_PROFILE`. It is needed to Github Actions, to deploy app to WebApp in Azure.
  - `Github repository >> Settings >> Secrets and variables >> Actions >> AZURE_WEBAPP_PUBLISH_PROFILE`
- Register the app in Azure:
  - Open **App registrations** view in Azure and register new app
    - `App registrations >> New registration`
    - Enter app name
    - Select `Accounts in this organizational directory only (Default Directory only - Single tenant)`
    - Redirect URI >> Web >> Link: `https://webapp-documentvault-ne.azurewebsites.net/signin-oidc`
    - Set `ID tokens (used for implicit and hybrid flows)`
      - `App registrations >> AZ204-DocumentVault >> Authentication >> Section: Implicit grant and hybrid flows`
  - Add `Directory (tenant) ID` and `Application (client) ID` to Github project secrets. You can find it on `Overview` page of `AZ204-DocumentVault` registration page
    - `Github repository >> Settings >> Secrets and variables >> Actions`:
      - `CLIENTID` secret
      - `TENANTID` secret
  - Deploy app to Azure using Github Actions
    - `Github repository >> Actions >> Workflows: Deploy to Azure Web App >> Run workflow`

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

### Azure Functions

- https://stackoverflow.com/a/55133438
- https://www.voitanos.io/blog/how-to-create-azure-function-apps-with-bicep-step-by-step/
- https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-bicep?tabs=CLI

### Azure Functions

- https://www.youtube.com/watch?v=82QnxMp8PRY

### Azure Functions

<<<<<<< HEAD
- https://www.youtube.com/watch?v=82QnxMp8PRY
=======
- https://stackoverflow.com/a/55133438
>>>>>>> a7304ed (Update README.md)

### Others

- https://github.com/AzureAD/microsoft-identity-web/wiki/web-apps
- https://brettmckenzie.net/posts/the-input-content-is-invalid-because-the-required-properties-id-are-missing/
- https://learn.microsoft.com/en-us/troubleshoot/azure/active-directory/error-code-aadsts50011-redirect-uri-mismatch
- https://stackoverflow.com/questions/43914151/azure-functions-i-cannot-choose-consumption-plan
