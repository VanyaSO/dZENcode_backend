namespace dZENcode_backend.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string? HomePage { get; set; }
    public ICollection<Comment> Comments { get; set; }
}