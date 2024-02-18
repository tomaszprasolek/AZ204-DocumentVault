using AZ204_DocumentVault.Services;
using AZ204_DocumentVault.Services.Models;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AZ204_DocumentVault.Pages;

public class Upload : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IStorageAccountService _storageAccountService;

    public string Message { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;

    public List<Document> Documents { get; set; } = new();
    public string? DocumentDownloadLink { get; set; }
    
    public Upload(ILogger<IndexModel> logger, 
        IOptions<AzureConfig> azureConfig,
        ICosmosDbService cosmosDbService,
        IStorageAccountService storageAccountService
        )
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _storageAccountService = storageAccountService;
    }

    public async Task<IActionResult> OnGet()
    {
        Documents = await _cosmosDbService.GetDocumentsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDownloadFile(string fileName)
    {
        Azure.Response<BlobDownloadInfo> blob = (await _storageAccountService.GetBlobAsync(fileName))!;

        return File(blob.Value.Content, blob.Value.ContentType, fileName);
    }

    public async Task<IActionResult> OnPostGenerateLink(string id, string fileName, int hoursToBeExpired)
    {
        DocumentDownloadLink = await _storageAccountService.GenerateDownloadLink(fileName, hoursToBeExpired);
        
        await _cosmosDbService.UpdateDocument<Document>(id, fileName, hoursToBeExpired);
        
        _logger.LogInformation("Generated link for file: {FileName}, expired after: {hoursToBeExpired}", 
            fileName, hoursToBeExpired);
        
        return await OnGet();
    }

    public async Task<IActionResult> OnPostDeleteFile(string id, string fileName)
    {
        await _storageAccountService.DeleteBlobAsync(fileName);
        
        await _cosmosDbService.DeleteDocument<Document>(id);

        Message = $"File {fileName} deleted";
        return await OnGet();
    }

    public async Task<IActionResult> OnPostAsync()
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
                await _storageAccountService.UploadBlobAsync(postedFile.FileName, postedFile.ContentType,
                    postedFile.OpenReadStream());
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

                await _cosmosDbService.AddDocument(document);
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
        
        return await OnGet();
    }
}