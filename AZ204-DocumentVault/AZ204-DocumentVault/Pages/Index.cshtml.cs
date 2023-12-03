﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

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
                string connectionString =
                    $"DefaultEndpointsProtocol=https;AccountName={_azureConfig.StorageAccountName};AccountKey={_azureConfig.StorageAcccountKey};EndpointSuffix=core.windows.net";
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
                Message = e.ToString();
            }
        }
        else
        {
            Message = "No file uploaded";
        }
    }
}

public class AzureConfig
{
    public string StorageAccountName { get; set; }
    public string StorageAcccountKey { get; set; }
    public string ContainerName { get; set; }
}