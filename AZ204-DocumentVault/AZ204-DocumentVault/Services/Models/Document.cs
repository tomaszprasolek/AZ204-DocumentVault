using Newtonsoft.Json;

namespace AZ204_DocumentVault.Services.Models;

public sealed class Document
{
    [JsonProperty("id")]
    public string Id { get; }

    [JsonProperty("userId")]
    public string UserId { get; }
    public string Name { get; }
    public string FileName { get; }
    public string UserName { get; }
    public string[]? Tags { get; } = Array.Empty<string>();

    public FileLink[]? FileLinks { get; set; } = Array.Empty<FileLink>();

    [JsonConstructor]
    public Document(string id,
        string userId,
        string name,
        string fileName,
        string userName,
        string[]? tags,
        FileLink[]? fileLinks)
    {
        Id = id;
        UserId = userId;
        Name = name;
        FileName = fileName;
        UserName = userName;
        Tags = tags;
        FileLinks = fileLinks;
    }

    public Document(string id,
        string userId,
        string name, string fileName, string tagsCommaSeparated, string userName)
    {
        Id = id;
        UserId = userId;
        Name = name;
        FileName = fileName;
        UserName = userName;
        if (!string.IsNullOrWhiteSpace(tagsCommaSeparated))
            Tags = tagsCommaSeparated.Split(',')
                .Select(x => x.Trim())
                .ToArray();
    }

    public string GetTags()
    {
        if (Tags is null)
            return string.Empty;

        if (Tags.Length == 0)
            return string.Empty;
        
        return Tags.Aggregate((a,
                b) => $"{a}, {b}");
    }

    public void AddLink(FileLink link)
    {
        if (FileLinks is null)
            FileLinks = new[] {link};
        else
        {
            FileLinks[FileLinks.Length + 1] = link;
        }
    }
}