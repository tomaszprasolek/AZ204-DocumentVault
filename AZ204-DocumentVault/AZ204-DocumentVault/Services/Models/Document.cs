using System.Text.Json.Serialization;

namespace AZ204_DocumentVault.Services.Models;

public sealed class Document
{
    [JsonPropertyName("id")]
    public string Id { get; }
    public string Name { get; }
    public string FileName { get; }
    public string[]? Tags { get; }
    
    public FileLink[]? FileLinks { get; private set; }

    [JsonConstructor]
    public Document(string id,
        string name,
        string fileName,
        string[]? tags,
        FileLink[]? fileLinks)
    {
        Id = id;
        Name = name;
        FileName = fileName;
        Tags = tags;
        FileLinks = fileLinks;
    }

    public Document(string id, string name, string fileName, string tagsCommaSeparated)
    {
        Id = id;
        Name = name;
        FileName = fileName;
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