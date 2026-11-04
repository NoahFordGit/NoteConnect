using Microsoft.EntityFrameworkCore;
using NoteConnect.Models.Entities;
namespace NoteConnect.Services;

using NoteConnect.Data;
using NoteConnect.Services;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    } // End method

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    } // End method

    public async Task<IEnumerable<User>> ReadAllAsync()
    {
        return await _context.Users
            .ToListAsync();
    } // End method

    public async Task<User?> ReadAsync(string UserId)
    {
        return await _context.Users 
            .FirstOrDefaultAsync(); 
    } // End method

    public async Task<User?> UpdateAsync(User user)
    {
        var existing = await _context.Users.FindAsync(user.Id);
        if (existing == null) 
        {
            return null;
        } // End if

        existing.UserName = user.UserName;
        existing.FirstName = user.FirstName;
        existing.LastName = user.LastName;
        existing.Email = user.Email;
        existing.DateCreated = user.DateCreated;

        await _context.SaveChangesAsync();
        return existing;

    } // End method

    public async Task<bool> DeleteAsync(string UserId)
    {
        var existing = await _context.Users.FindAsync(UserId);
        if (existing == null) 
        {
            return false;
        } // End if

        _context.Users.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    } // End method

} // End repo
