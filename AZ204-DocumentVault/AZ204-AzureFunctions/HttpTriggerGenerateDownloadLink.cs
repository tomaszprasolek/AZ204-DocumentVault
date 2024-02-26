using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AZ204_AzureFunctions;

public static class HttpTriggerGenerateDownloadLink
{
    [FunctionName("HttpGenerateDownloadLink")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        //[FromBody] BlobInfo blobInfo,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        BlobInfo blobInfo = await JsonSerializer.DeserializeAsync<BlobInfo>(req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        log.LogInformation(blobInfo.ToString());
        
        // string link = GenerateLink(blobInfo);
        string link = "test001";

        return new OkObjectResult(link);
    }

    private static string GenerateLink(BlobInfo blobInfo)
    {
        return "test-link";
        
        BlobContainerClient containerClient = GetBlobContainerClient();

        BlobClient blobClient = containerClient.GetBlobClient(blobInfo.FileName);

        DateTime expiresOn = DateTime.UtcNow.AddHours(blobInfo.HoursToBeExpired);
        Uri result = blobClient.GenerateSasUri(BlobSasPermissions.Read, new DateTimeOffset(expiresOn));
        return result.ToString();
    }

    private static BlobContainerClient GetBlobContainerClient()
    {
        string storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
        string storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
        string containerName = Environment.GetEnvironmentVariable("ContainerName");
        
        string connectionString =
            $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        return containerClient;
    }
}

public class BlobInfo
{
    public string FileName { get; set; }
    public int HoursToBeExpired { get; set; }

    public override string ToString()
    {
        return $"File name: {FileName}, time left (in hours) when link expired: {HoursToBeExpired}";
    }
}