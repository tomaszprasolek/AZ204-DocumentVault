using AZ204_DocumentVault.Services.Models;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace AZ204_DocumentVault.Services;

public interface IStorageAccountService
{
    Task<Response<BlobDownloadInfo>?> GetBlobAsync(string fileName);
    Task DeleteBlobAsync(string fileName);

    Task UploadBlobAsync(string fileName,
        string contentType,
        Stream content);

    Task<string> GenerateDownloadLink(string fileName,
        int hoursToBeExpired);
}

public sealed class StorageAccountService : IStorageAccountService
{
    private readonly AzureConfig _azureConfig;
    private readonly IKeyVaultService _keyVault;
    
    public StorageAccountService(IOptions<AzureConfig> azureConfig,
        IKeyVaultService keyVault)
    {
        _azureConfig = azureConfig.Value;
        _keyVault = keyVault;
    }

    public async Task<Response<BlobDownloadInfo>?> GetBlobAsync(string fileName)
    {
        BlobContainerClient containerClient = await GetBlobContainerClientAsync();

        BlobClient? blobClient = containerClient.GetBlobClient(fileName);

        Response<BlobDownloadInfo>? blob = await blobClient.DownloadAsync();

        return blob;
    }

    public async Task DeleteBlobAsync(string fileName)
    {
        BlobContainerClient containerClient = await GetBlobContainerClientAsync();
        await containerClient.DeleteBlobAsync(fileName);
    }

    public async Task UploadBlobAsync(string fileName, string contentType, Stream content)
    {
        BlobContainerClient containerClient = await GetBlobContainerClientAsync();

        BlobClient blobClient = containerClient.GetBlobClient(fileName);

        var blobHttpHeaders = new BlobHttpHeaders();
        blobHttpHeaders.ContentType =contentType;

        await blobClient.UploadAsync(content, blobHttpHeaders);
    }

    public async Task<string> GenerateDownloadLink(string fileName, int hoursToBeExpired)
    {
        BlobContainerClient containerClient = await GetBlobContainerClientAsync();

        BlobClient? blobClient = containerClient.GetBlobClient(fileName);

        DateTime expiresOn = DateTime.UtcNow.AddHours(hoursToBeExpired);
        Uri? result = blobClient.GenerateSasUri(BlobSasPermissions.Read, new DateTimeOffset(expiresOn));

        return result.ToString();
    }
    
    private async Task<BlobContainerClient> GetBlobContainerClientAsync()
    {
        string storageAccountKey = await _keyVault.GetSecretAsync("StorageAccountKey");
        string connectionString =
            $"DefaultEndpointsProtocol=https;AccountName={_azureConfig.StorageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_azureConfig.ContainerName);
        return containerClient;
    }
}