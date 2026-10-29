using Microsoft.EntityFrameworkCore;
using NoteConnect.Data;
using NoteConnect.Models.Entities;

namespace NoteConnect.Services;

public class NoteRepository(ApplicationDbContext context) : INoteRepository
{
    public Task<List<Note>> GetAllAsync(string userId) => context.Notes
        .AsNoTracking()
        .Where(n => n.UserId == userId)
        .OrderByDescending(n => n.DateCreated)
        .ThenByDescending(n => n.NoteId)
        .ToListAsync();

    public Task<Note?> GetByIdAsync(int noteId, string userId) => context.Notes
        .AsNoTracking()
        .SingleOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

    public async Task CreateAsync(Note note, string userId)
    {
        // Copy only editable fields; generate identity, owner, and timestamp server-side.
        context.Notes.Add(new Note
        {
            NoteTitle = note.NoteTitle.Trim(),
            Content = note.Content,
            NotebookId = note.NotebookId,
            UserId = userId,
            DateCreated = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int noteId, Note input, string userId)
    {
        var note = await context.Notes
            .SingleOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

        if (note is null)
            return false;

        note.NoteTitle = input.NoteTitle.Trim();
        note.Content = input.Content;
        note.NotebookId = input.NotebookId;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int noteId, string userId)
    {
        var note = await context.Notes
            .SingleOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

        if (note is null)
            return false;

        context.Notes.Remove(note);
        await context.SaveChangesAsync();
        return true;
    }
}
