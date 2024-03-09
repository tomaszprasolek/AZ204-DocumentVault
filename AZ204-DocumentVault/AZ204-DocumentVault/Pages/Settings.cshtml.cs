using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AZ204_DocumentVault.Pages;

public class Settings : PageModel
{
    private readonly IConfiguration _configuration;

    public List<string> AppSettings { get; set; } = new();

    public Settings(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void OnGet()
    {
        AppSettings.Add($"AzureConfig:FunctionApp:GenerateDownloadMethodFunctionKey: {_configuration.GetValue<string>("AzureConfig:FunctionApp:GenerateDownloadMethodFunctionKey")}");
        AppSettings.Add($"AzureAd:TenantId: {_configuration.GetValue<string>("AzureAd:TenantId")}");
        AppSettings.Add($"AzureAd:ClientId: {_configuration.GetValue<string>("AzureAd:ClientId")}");
    }
}