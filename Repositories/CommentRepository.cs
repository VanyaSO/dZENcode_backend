using dZENcode_backend.Data;
using dZENcode_backend.Interfaces;
using dZENcode_backend.Models;
using dZENcode_backend.Models.Pages;
using Microsoft.EntityFrameworkCore;

namespace dZENcode_backend.Repositories;

public class CommentRepository : IComment
{
    private readonly ApplicationContext _context;

    public CommentRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task AddComment(Comment comment)
    {
        await _context.AddAsync(comment);
        await _context.SaveChangesAsync();
    }
    
    public async Task<Comment?> GetCommentByIdAsync(Guid id) => await _context.Comments
        .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<Comment>> GetRepliesWithUserByIdAsync(Guid id) => await _context.Comments
        .Where(e => e.ParentId == id)
        .Include(e => e.Parent)
        .Include(e => e.User)
        .ToListAsync();

    public async Task<PagedList<Comment>> GetAllMainCommentsAsync(QueryOptions options) =>
        await PagedList<Comment>.CreateAsync(_context.Comments
            .Include(e => e.User)
            .Where(e => e.ParentId == null), options);
    

    public async Task UpdateRepliesCountAsync(Comment comment, int count)
    {
        comment.RepliesCount += count;
        await _context.SaveChangesAsync();
    }
}