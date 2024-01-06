using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AZ204_DocumentVault.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private AzureConfig _azureConfig;

    public string Message { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;

    public List<Document> Documents { get; set; } = new();
    public string DocumentDownloadLink { get; set; }
    
    
    public IndexModel(ILogger<IndexModel> logger, IOptions<AzureConfig> config)
    {
        _logger = logger;
        _azureConfig = config.Value;
    }

    public async Task<IActionResult> OnGet()
    {
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
                Documents.Add(item);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadFile(string fileName)
    {
        BlobContainerClient containerClient = await GetBlobContainerClient();

        BlobClient? blobClient = containerClient.GetBlobClient(fileName);

        var blob = await blobClient.DownloadAsync();

        var stream = await blobClient.OpenReadAsync();
        return File(stream, blob.Value.ContentType, fileName);
    }

    public async Task<IActionResult> OnPostGenerateLink(string id, string fileName, int hoursToBeExpired)
    {
        BlobContainerClient containerClient = await GetBlobContainerClient();

        BlobClient? blobClient = containerClient.GetBlobClient(fileName);

        DateTime expiresOn = DateTime.UtcNow.AddHours(hoursToBeExpired);
        Uri? result = blobClient.GenerateSasUri(BlobSasPermissions.Read, new DateTimeOffset(expiresOn));
        DocumentDownloadLink = result.ToString();
        
        Container container = await GetCosmosDbContainerAsync();
        ItemResponse<Document>? response = await container.ReadItemAsync<Document>(id, new PartitionKey(id));
        Document document = response.Resource;
        
        document.AddLink(new FileLink(DocumentDownloadLink, expiresOn));

        await container.UpsertItemAsync(document, new PartitionKey(id));
        
        _logger.LogInformation("Generated link for file: {FileName}, expired after: {hoursToBeExpired}", 
            fileName, hoursToBeExpired);
        
        return await OnGet();
    }

    public async Task OnPostAsync()
    {
        DocumentName = Request.Form["documentName"]!;

        string tagsField = Request.Form["tags"]!;
        Tags = tagsField.Split(Environment.NewLine).Aggregate((a,
                b) => $"{a}, {b}");

        if (Request.Form.Files.Count > 0)
        {
            IFormFile postedFile = Request.Form.Files["documentFile"]!;

            try
            {
                BlobContainerClient containerClient = await GetBlobContainerClient();

                BlobClient blobClient = containerClient.GetBlobClient(postedFile.FileName);

                var blobHttpHeaders = new BlobHttpHeaders();
                blobHttpHeaders.ContentType = postedFile.ContentType;

                await blobClient.UploadAsync(postedFile.OpenReadStream(), blobHttpHeaders);
                Message = "File uploaded";
            }
            catch (Exception e)
            {
                Message = "Storage account error" + Environment.NewLine;
                Message += e.ToString();
            }

            try
            {
                // Save document data: name and tags
                var document = new Document(Guid.NewGuid().ToString(), 
                    DocumentName, 
                    postedFile.FileName, 
                    tagsField);
                
                var container = await GetCosmosDbContainerAsync();
                
                await container.CreateItemAsync(document);
            }
            catch (Exception e)
            {
                Message = "Cosmos DB error" + Environment.NewLine;
                Message += e.ToString();
            }
        }
        else
        {
            Message = "No file uploaded";
        }
    }

    private async Task<BlobContainerClient> GetBlobContainerClient()
    {
        string storageAccountKey = await GetSecretFromKeyVault("StorageAccountKey");
        string connectionString =
            $"DefaultEndpointsProtocol=https;AccountName={_azureConfig.StorageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_azureConfig.ContainerName);
        return containerClient;
    }

    private async Task<string> GetSecretFromKeyVault(string secretName)
    {
        var secretClient = new SecretClient(new Uri(_azureConfig.KeyVaultUri), new DefaultAzureCredential());
        Azure.Response<KeyVaultSecret>? secretResponse = await secretClient.GetSecretAsync(secretName);
        return secretResponse.Value.Value;
    }
    
    private async Task<Container> GetCosmosDbContainerAsync()
    {
        string key = await GetSecretFromKeyVault("CosmosDbKey");
        
        string connectionString = $"AccountEndpoint={_azureConfig.CosmosDbUri};AccountKey={key}";
        CosmosClient cosmosClient = new CosmosClient(connectionString);

        Database? db = cosmosClient.GetDatabase("DocumentsVault");
        Container? container = db.GetContainer("Documents");
        return container;
    }
}

public class AzureConfig
{
    public string StorageAccountName { get; set; } = string.Empty;
    public string StorageAccountKey { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string KeyVaultUri { get; set; } = string.Empty;
    public string CosmosDbUri { get; set; } = string.Empty;
}

// To use in Azure Cosmos DB
public sealed class Document
{
    [JsonProperty("id")]
    public string Id { get; }
    public string Name { get; }
    public string FileName { get; }
    public string[]? Tags { get; }
    
    public FileLink[]? FileLinks { get; private set; }

    public Document(string id, string name, string fileName, string tagsCommaSeparated)
    {
        Id = id;
        Name = name;
        FileName = fileName;
        if (!string.IsNullOrWhiteSpace(tagsCommaSeparated))
            Tags = tagsCommaSeparated.Split(',')
                .Select(x => x.Trim())
                .ToArray();
    }

    public void AddLink(FileLink link)
    {
        if (FileLinks is null)
            FileLinks = new[] {link};
        else
        {
            FileLinks[FileLinks.Length + 1] = link;
        }
    }
}

public sealed class FileLink
{
    public string Url { get; }
    public DateTime Expiration { get; }

    public FileLink(string url, DateTime expiration)
    {
        Url = url;
        Expiration = expiration;
    }
}