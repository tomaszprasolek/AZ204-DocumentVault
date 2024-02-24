using AZ204_DocumentVault.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AZ204_DocumentVault.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    public string TestConfig { get; set; }
    
    public IndexModel(ILogger<IndexModel> logger, 
        IOptions<AzureConfig> azureConfig,
        IConfiguration configuration
        ) 
    {
        _logger = logger;

        string tenantId = configuration.GetSection("AzureAd:TenantId").Value!;
        string clientId = configuration.GetSection("AzureAd:ClientId").Value!;
        TestConfig = $"Client id: {clientId}, tenant id: {tenantId}";
    }

    public void OnGet()
    {
        _logger.LogInformation("Loan index page");
    }
}