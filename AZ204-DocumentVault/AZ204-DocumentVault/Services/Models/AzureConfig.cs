namespace AZ204_DocumentVault.Services.Models;

public sealed class AzureConfig
{
    public string StorageAccountName { get; set; } = string.Empty;
    public string StorageAccountKey { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string KeyVaultUri { get; set; } = string.Empty;
    public string CosmosDbUri { get; set; } = string.Empty;
    public FunctionApp FunctionApp { get; set; }
}