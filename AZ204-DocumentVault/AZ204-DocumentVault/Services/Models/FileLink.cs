namespace AZ204_DocumentVault.Services.Models;

public sealed class FileLink
{
    public string Url { get; }
    public DateTime Expiration { get; }

    public FileLink(string url, DateTime expiration)
    {
        Url = url;
        Expiration = expiration;
    }
}