namespace dZENcode_backend.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ParentId { get; set; }
    public int RepliesCount { get; set; } = 0;
    public FileInfo? Image { get; set; }
    public FileInfo? Document { get; set; }
    public ICollection<Comment> Replies { get; set; }
    public Comment? Parent { get; set; }
}