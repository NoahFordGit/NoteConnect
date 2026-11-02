using Microsoft.EntityFrameworkCore;
using NoteConnect.Models.Enities;
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
        _context.User.Add(user);
        await _context.SaveChangesAsync();
        return user;
    } // End method

    public async Task<IEnumerable<User>> ReadAllAsync()
    {
        return await _context.User
            .ToListAsync();
    } // End method

    public async Task<User?> ReadAsync(int UserId)
    {
        return await _context.User 
            .FirstOrDefaultAsync(); 
    } // End method

    public async Task<User?> UpdateAsync(User user)
    {
        var existing = await _context.User.FindAsync(user.UserId);
        if (existing == null) 
        {
            return null;
        } // End if

        existing.Username = user.Username;
        existing.FirstName = user.FirstName;
        existing.LastName = user.LastName;
        existing.Email = user.Email;
        existing.DateCreated = user.DateCreated;

        await _context.SaveChangesAsync();
        return existing;

    } // End method

    public async Task<bool> DeleteAsync(int UserId)
    {
        var existing = await _context.User.FindAsync(UserId);
        if (existing == null) 
        {
            return false;
        } // End if

        _context.User.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    } // End method

} // End repo
