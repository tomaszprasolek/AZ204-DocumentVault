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
    
    public IndexModel(ILogger<IndexModel> logger, IOptions<AzureConfig> azureConfig) 
    {
        _logger = logger;
        TestConfig = azureConfig.Value.Test;
    }

    public void OnGet()
    {
        _logger.LogInformation("Loan index page");
    }
}