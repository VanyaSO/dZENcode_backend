using dZENcode_backend.Data;
using dZENcode_backend.Interfaces;
using dZENcode_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace dZENcode_backend.Repositories;

public class UserRepository : IUser
{
    private readonly ApplicationContext _context;

    public UserRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailOrUsernameAsync(string email, string username) => await  _context.Users
        .FirstOrDefaultAsync(u => u.Email == email || u.Username == username);

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
     
}