using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AZ204_AzureFunctions;

public static class HttpTriggerGenerateDownloadLink
{
    [FunctionName("HttpGenerateDownloadLink")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        [FromBody] BlobInfo blobInfo,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        BlobContainerClient containerClient = GetBlobContainerClient();

        BlobClient blobClient = containerClient.GetBlobClient(blobInfo.FileName);

        DateTime expiresOn = DateTime.UtcNow.AddHours(blobInfo.HoursToBeExpired);
        Uri result = blobClient.GenerateSasUri(BlobSasPermissions.Read, new DateTimeOffset(expiresOn));
        
        // string name = req.Query["name"];
        //
        // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        // dynamic data = JsonConvert.DeserializeObject(requestBody);
        // name = name ?? data?.name;
        //
        // return name != null
        //     ? (ActionResult) new OkObjectResult($"Hello, {name}")
        //     : new BadRequestObjectResult("Please pass a name on the query string or in the request body");

        return new OkObjectResult(result.ToString());
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
}