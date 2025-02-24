using FileInfo = dZENcode_backend.Models.FileInfo;

namespace dZENcode_backend.Helpers;

public static class ApplicationExtensions
{
    public static FileInfo BuildFullPath(this FileInfo fileInfo, HttpRequest request)
    {
        return new FileInfo()
        {
            Path = $"{request.Scheme}://{request.Host}" + fileInfo.Path,
            Name = fileInfo.Name
        };
    }

    public static string GetMimeType(this string fileName)
    {
        return Path.GetExtension(fileName).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain"
        };
    }
}