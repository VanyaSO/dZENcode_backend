namespace dZENcode_backend.Models.Pages;

public class QueryOptions
{
    public int Page { get; set; } = 1;
    public string? OrderBy { get; set; }
    public bool Desc { get; set; }
}