using SixLabors.ImageSharp;

namespace dZENcode_backend.Helpers;

public class FileSaver
{
    private readonly IWebHostEnvironment _appEnvironment;

    public FileSaver(IWebHostEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
    }
    
    public async Task SaveFileAsync(IFormFile file, string dirPath)
    {
        string fileName = file.FileName, filePath;
        
        if (fileName.Contains("\\"))
            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
 
        filePath = dirPath + Guid.NewGuid() + fileName;
 
        using (var fileStream = new FileStream(_appEnvironment.WebRootPath + filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
    }
    
    public async Task SaveFileAsync(Image file, string dirPath, string fileName)
    {
        string filePath;
        
        if (fileName.Contains("\\"))
            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
 
        filePath = dirPath + Guid.NewGuid() + fileName;
        
        await file.SaveAsync(_appEnvironment.WebRootPath + filePath);
    }
}