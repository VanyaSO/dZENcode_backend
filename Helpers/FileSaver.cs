using SixLabors.ImageSharp;
using FileInfo = dZENcode_backend.Models.FileInfo;

namespace dZENcode_backend.Helpers;

public class FileSaver
{
    private readonly IWebHostEnvironment _appEnvironment;

    public FileSaver(IWebHostEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
    }

    public async Task<FileInfo> SaveFileAsync(IFormFile file, string dirName)
    {
        CreateDirectoryIfNotExists(dirName);
        string fileName = GetFileName(file.FileName);
        string filePath = GenerateFilePath(dirName, fileName);

        using (var fileStream = new FileStream(_appEnvironment.WebRootPath + filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return new FileInfo(filePath, fileName);
    }

    public async Task<FileInfo> SaveFileAsync(Image file, string dirName, string fileName)
    {
        CreateDirectoryIfNotExists(dirName);
        string filePath = GenerateFilePath(dirName, fileName);

        await file.SaveAsync(_appEnvironment.WebRootPath + filePath);
        return new FileInfo(filePath, fileName);
    }

    private void CreateDirectoryIfNotExists(string dirName)
    {
        string dirPath = Path.Combine(_appEnvironment.WebRootPath, dirName);

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
    }

    private string GetFileName(string fileName) => Path.GetFileName(fileName);

    private string GenerateFilePath(string dirPath, string fileName) => $"/{dirPath}/{Guid.NewGuid()}{fileName}";
}