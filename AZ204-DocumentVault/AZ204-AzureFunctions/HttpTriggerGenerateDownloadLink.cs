using AZ204_AzureFunctions.Models;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace AZ204_AzureFunctions;

public static class HttpTriggerGenerateDownloadLink
{
    [FunctionName("GenerateDownloadLink")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        BlobInfo blobInfo = await JsonSerializer.DeserializeAsync<BlobInfo>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        log.LogInformation(blobInfo.ToString());
        
        string link = await GenerateLink(blobInfo);

        return new OkObjectResult(link);
    }

    private static async Task<string> GenerateLink(BlobInfo blobInfo)
    {
        BlobContainerClient containerClient = await GetBlobContainerClientAsync();

        BlobClient blobClient = containerClient.GetBlobClient(blobInfo.FileName);

        DateTime expiresOn = DateTime.UtcNow.AddHours(blobInfo.HoursToBeExpired);
        Uri result = blobClient.GenerateSasUri(BlobSasPermissions.Read, new DateTimeOffset(expiresOn));
        return result.ToString();
    }

    private static async Task<BlobContainerClient> GetBlobContainerClientAsync()
    {
        string storageAccountKey = await GetSecretAsync("StorageAccountKey");
        string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
        string containerName = Environment.GetEnvironmentVariable("ContainerName");
        
        string connectionString =
            $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        return containerClient;
    }

    private static async Task<string> GetSecretAsync(string secretName)
    {
        string keyVaultUri = Environment.GetEnvironmentVariable("KeyVaultUri")!;
        var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        Azure.Response<KeyVaultSecret> secretResponse = await secretClient.GetSecretAsync(secretName);
        return secretResponse.Value.Value;
    }
}