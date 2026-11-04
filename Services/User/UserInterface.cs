using Microsoft.EntityFrameworkCore;
using NoteConnect.Models.Entities;
namespace NoteConnect.Services;

public interface IUserRepository
{
    Task<IEnumerable<User>> ReadAllAsync();
    Task<User?> ReadAsync(string UserId);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(User user);
    Task<bool> DeleteAsync(User user);
     
} // End interface