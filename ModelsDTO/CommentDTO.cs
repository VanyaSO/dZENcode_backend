namespace dZENcode_backend.ModelsDTO;

public class CommentDTO
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string? HomePage { get; set; }
    public string Content { get; set; }
    public string CreatedAt { get; set; }
    public string? ParentId { get; set; }
    public int RepliesCount { get; set; } = 0;
}