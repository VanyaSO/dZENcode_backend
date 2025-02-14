using dZENcode_backend.Models;

namespace dZENcode_backend.Interfaces;

public interface IUser
{
     Task<User?> GetUserByEmailOrUsernameAsync(string email, string username);
     Task AddUserAsync(User user);
     Task UpdateUserAsync(User user);
}