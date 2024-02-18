using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AZ204_DocumentVault.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    
    public IndexModel(ILogger<IndexModel> logger) 
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Loan index page");
    }
}