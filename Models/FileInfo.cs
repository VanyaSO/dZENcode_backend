using Microsoft.EntityFrameworkCore;

namespace dZENcode_backend.Models;

[Owned]
public class FileInfo
{
    public string Path { get; set; }
    public string Name { get; set; }

    public FileInfo(){}
    public FileInfo(string path, string name)
    {
        Path = path;
        Name = name;
    }
}