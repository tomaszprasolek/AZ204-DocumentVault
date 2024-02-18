using AZ204_DocumentVault.Services.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace AZ204_DocumentVault.Services;

public interface ICosmosDbService
{
    Task<List<Document>> GetDocumentsAsync();
    Task AddDocument(Document document);
    Task DeleteDocument<T>(string id);

    Task UpdateDocument<T>(string id,
        string documentDownloadLink,
        int hoursToBeExpired);
}

public sealed class CosmosDbService : ICosmosDbService
{
    private readonly AzureConfig _azureConfig;
    private readonly IKeyVaultService _keyVault;

    public CosmosDbService(IOptions<AzureConfig> azureConfig,
        IKeyVaultService keyVault)
    {
        _azureConfig = azureConfig.Value;
        _keyVault = keyVault;
    }
    
    public async Task<List<Document>> GetDocumentsAsync()
    {
        List<Document> documents = new List<Document>();
        
        Container container = await GetCosmosDbContainerAsync();

        IOrderedQueryable<Document>? queryable = container.GetItemLinqQueryable<Document>();
        
        // Convert to feed iterator
        using FeedIterator<Document> linqFeed = queryable.ToFeedIterator();

        while (linqFeed.HasMoreResults)
        {
            FeedResponse<Document> response = await linqFeed.ReadNextAsync();
            
            // Iterate query results
            foreach (Document item in response)
            {
                documents.Add(item);
            }
        }

        return documents;
    }

    public async Task AddDocument(Document document)
    {
        var container = await GetCosmosDbContainerAsync();
                
        await container.CreateItemAsync(document);
    }

    public async Task DeleteDocument<T>(string id)
    {
        Container container = await GetCosmosDbContainerAsync();
        await container.DeleteItemAsync<T>(id, new PartitionKey(id));
    }

    public async Task UpdateDocument<T>(string id, string documentDownloadLink, int hoursToBeExpired)
    {
        Container container = await GetCosmosDbContainerAsync();
        await container.PatchItemAsync<T>(id, new PartitionKey(id), new List<PatchOperation>
        {
            PatchOperation.Add("/FileLinks/-", new FileLink(documentDownloadLink, DateTime.UtcNow.AddHours(hoursToBeExpired)))
        });
    }
    
    private async Task<Container> GetCosmosDbContainerAsync()
    {
        string key = await _keyVault.GetSecretAsync("CosmosDbKey");
        
        string connectionString = $"AccountEndpoint={_azureConfig.CosmosDbUri};AccountKey={key}";
        CosmosClient cosmosClient = new CosmosClient(connectionString);

        Database? db = cosmosClient.GetDatabase("DocumentsVault");
        Container? container = db.GetContainer("Documents");
        return container;
    }
}