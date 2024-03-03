﻿using AZ204_DocumentVault.Services;
using AZ204_DocumentVault.Services.Models;
using Azure.Storage.Blobs.Models;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Text.Json;
using System.Web;

namespace AZ204_DocumentVault.Pages;

public class Upload : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly IStorageAccountService _storageAccountService;
    private readonly HttpClient _httpClient;
    private readonly AzureConfig _azureConfig;

    public string Message { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;

    public List<Document> Documents { get; set; } = new();
    public string? DocumentDownloadLink { get; set; }

    private string UserId => User.GetObjectId()!;

    public Upload(ILogger<IndexModel> logger, 
        IOptions<AzureConfig> azureConfig,
        ICosmosDbService cosmosDbService,
        IStorageAccountService storageAccountService,
        IHttpClientFactory httpClientFactory
        )
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _storageAccountService = storageAccountService;
        _httpClient = httpClientFactory.CreateClient("AzureFunctionsClient");
        _azureConfig = azureConfig.Value;

    }

    public async Task<IActionResult> OnGet()
    {
        Documents = await _cosmosDbService.GetDocumentsAsync(UserId);
        return Page();
    }

    public async Task<IActionResult> OnPostDownloadFile(string fileName)
    {
        Azure.Response<BlobDownloadInfo> blob = (await _storageAccountService.GetBlobAsync(fileName))!;

        return File(blob.Value.Content, blob.Value.ContentType, fileName);
    }

    public async Task<IActionResult> OnPostGenerateLink(string id, string fileName, int hoursToBeExpired)
    {
        _logger.LogInformation("Start: {Method}", nameof(OnPostGenerateLink));
        // DocumentDownloadLink = await _storageAccountService.GenerateDownloadLink(fileName, hoursToBeExpired);

        string url = _azureConfig.FunctionApp.GenerateDownloadFunctionLink
            .SetQueryParam("code", _azureConfig.FunctionApp.GenerateDownloadMethodFunctionKey);
        
        // HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url,
        //     new
        //     {
        //         FileName = fileName,
        //         HoursToBeExpired = hoursToBeExpired
        //     }, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // response.EnsureSuccessStatusCode();
        
        // DownloadLink? link = await response.Content.ReadFromJsonAsync<DownloadLink>();
        DownloadLink? link = await CallAzureFunctionAsync(fileName);
        DocumentDownloadLink = link!.Value;
        
        await _cosmosDbService.UpdateDocument<Document>(id, UserId, fileName, hoursToBeExpired);
        
        _logger.LogInformation("Generated link for file: {FileName}, expired after: {hoursToBeExpired}", 
            fileName, hoursToBeExpired);
        
        return await OnGet();
    }

    private async Task<DownloadLink> CallAzureFunctionAsync(string fileName)
    {
        _logger.LogInformation("Start: {Method}", nameof(CallAzureFunctionAsync));
        
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, HttpUtility.UrlEncode("https://functionapp-app.azurewebsites.net/api/GenerateDownloadLink?code=ndcC_p82_VFAfi3-G3Hg4EVkwfOEYs5aLFP8nPm-fZP5AzFuFTBUJg=="));
        
        var content = new StringContent($"{{\r\n    \"fileName\": \"{fileName}\",\r\n    \"hoursToBeExpired\": 8\r\n}}", null, "application/json");
        request.Content = content;
        _logger.LogInformation("Sending content: {Content}", content);
        
        HttpResponseMessage response = await client.SendAsync(request);
        _logger.LogInformation("Status code: {StatusCode}, content: {Content}", response.StatusCode, response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
        
        return (await response.Content.ReadFromJsonAsync<DownloadLink>())!;
    }

    public async Task<IActionResult> OnPostDeleteFile(string id, string fileName)
    {
        await _storageAccountService.DeleteBlobAsync(fileName);
        
        await _cosmosDbService.DeleteDocument<Document>(id, UserId);

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
                    UserId,
                    DocumentName, 
                    postedFile.FileName,
                    tagsField,
                    User.Identity!.Name!);

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