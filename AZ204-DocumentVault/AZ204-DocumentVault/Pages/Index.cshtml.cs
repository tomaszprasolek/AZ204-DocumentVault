using AZ204_DocumentVault.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AZ204_DocumentVault.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    
    public IndexModel(ILogger<IndexModel> logger, 
        IOptions<AzureConfig> azureConfig) 
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Loan index page");
    }
}