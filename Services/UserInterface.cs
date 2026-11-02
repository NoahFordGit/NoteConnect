using Microsoft.EntityFrameworkCore;
using NoteConnect.Models.Enities;
namespace NoteConnect.Services;

public interface IUserRepository
{
    Task<IEnumerable<User>> ReadAllAsync();
    Task<User?> ReadAsync(int UserId);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(User user);
    Task<bool> DeleteAsync(int UserId);
     
} // End interface