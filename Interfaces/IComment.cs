using dZENcode_backend.Models;
using dZENcode_backend.Models.Pages;

namespace dZENcode_backend.Interfaces;

public interface IComment
{
    Task AddComment(Comment comment);
    Task<Comment?> GetCommentByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetRepliesWithUserByIdAsync(Guid id);
    Task<PagedList<Comment>> GetAllMainCommentsAsync(QueryOptions options);
    Task UpdateRepliesCountAsync(Comment comment, int count);
}