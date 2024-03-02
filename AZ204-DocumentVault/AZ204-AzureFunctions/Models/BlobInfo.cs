namespace AZ204_AzureFunctions.Models;

public class BlobInfo
{
    public string FileName { get; set; }
    public int HoursToBeExpired { get; set; }

    public override string ToString()
    {
        return $"File name: {FileName}, time left (in hours) when link expired: {HoursToBeExpired}";
    }
}