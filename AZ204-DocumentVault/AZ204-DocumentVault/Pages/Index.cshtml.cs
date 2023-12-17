using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AZ204_DocumentVault.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public string Message { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;

    private AzureConfig _azureConfig;
    
    
    public IndexModel(ILogger<IndexModel> logger, IOptions<AzureConfig> config)
    {
        _logger = logger;
        _azureConfig = config.Value;
    }

    public void OnGet()
    {
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

            // using var ms = new MemoryStream();
            // postedFile.CopyTo(ms);
            // var fileBytes = ms.ToArray();
            // string s = Convert.ToBase64String(fileBytes);
            // // act on the Base64 data

            try
            {
                string storageAccountKey = await GetSecretFromKeyVault("StorageAccountKey");
                string connectionString =
                    $"DefaultEndpointsProtocol=https;AccountName={_azureConfig.StorageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_azureConfig.ContainerName);

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
                await GetContainer().CreateItemAsync(document);
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

    private async Task<string> GetSecretFromKeyVault(string secretName)
    {
        var secretClient = new SecretClient(new Uri(_azureConfig.KeyVaultURI), new DefaultAzureCredential());
        Azure.Response<KeyVaultSecret>? secretResponse = await secretClient.GetSecretAsync(secretName);
        return secretResponse.Value.Value;
    }
    
    private static Container GetContainer()
    {
        string cosmosDbUri = "https://cosdb-documentvault-ne.documents.azure.com:443/";
        string key = "vs1OfvRHqeXxk3U8sybqq1HFqMwR6ZLSjrAt7bSNlSqtZykrYFfN0tEbGPgeEmK67nHfvo27GjT1ACDbq2eTUA==";
        
        string connectionString = $"AccountEndpoint={cosmosDbUri};AccountKey={key}";
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
    public string KeyVaultURI { get; set; } = string.Empty;
}

public class Document
{
    [JsonProperty("id")]
    public string Id { get; }
    public string Name { get; }
    public string FileName { get; }
    public string[]? Tags { get; } = Array.Empty<string>();

    public Document(string id, string name, string fileName, string tagsCommaSeparated)
    {
        Id = id;
        Name = name;
        FileName = fileName;
        if (!string.IsNullOrWhiteSpace(tagsCommaSeparated))
            Tags = tagsCommaSeparated.Split(',');
    }
}