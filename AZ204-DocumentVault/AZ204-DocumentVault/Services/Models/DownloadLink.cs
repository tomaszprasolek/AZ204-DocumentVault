using System.Text.Json.Serialization;

namespace AZ204_DocumentVault.Services.Models;

public class DownloadLink
{
    [JsonPropertyName("DownloadLink")] 
    public string Value { get; set; } = string.Empty;
}