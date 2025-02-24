using FileInfo = dZENcode_backend.Models.FileInfo;

namespace dZENcode_backend.ModelsDTO;

public class CommentDTO
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? HomePage { get; set; }
    public string Content { get; set; }
    public string CreatedAt { get; set; }
    public CommentParentDTO? Parent { get; set; }
    public FileInfo? Image { get; set; }
    public FileInfo? Document { get; set; }
    public int RepliesCount { get; set; } = 0;
}

public class CommentParentDTO
{
    public string Username { get; set; }
    public string Content { get; set; }
}