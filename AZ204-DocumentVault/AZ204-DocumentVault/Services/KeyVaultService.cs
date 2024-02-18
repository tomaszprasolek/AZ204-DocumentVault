using AZ204_DocumentVault.Services.Models;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;

namespace AZ204_DocumentVault.Services;

public interface IKeyVaultService
{
    Task<string> GetSecretAsync(string secretName);
}

public sealed class KeyVaultService : IKeyVaultService
{
    private readonly AzureConfig _azureConfig;

    public KeyVaultService(IOptions<AzureConfig> azureConfig)
    {
        _azureConfig = azureConfig.Value;
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        var secretClient = new SecretClient(new Uri(_azureConfig.KeyVaultUri), new DefaultAzureCredential());
        Azure.Response<KeyVaultSecret>? secretResponse = await secretClient.GetSecretAsync(secretName);
        return secretResponse.Value.Value;
    }
}