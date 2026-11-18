using Microsoft.EntityFrameworkCore;
using NoteConnect.Data;
using NoteConnect.Models.Entities;

namespace NoteConnect.Services;

public class NotebookRepository(ApplicationDbContext context) : INotebookRepository
{
    public Task<List<Notebook>> GetAllAsync(string userId) => context.Notebooks
        .AsNoTracking()
        .Include(n => n.Notes.Where(note => note.UserId == userId))
        .Where(n => n.UserId == userId)
        .OrderByDescending(n => n.DateCreated)
        .ThenByDescending(n => n.NotebookId)
        .ToListAsync();

    public Task<Notebook?> GetByIdAsync(int notebookId, string userId) => context.Notebooks
        .AsNoTracking()
        .Include(n => n.Notes.Where(note => note.UserId == userId))
        .SingleOrDefaultAsync(n => n.NotebookId == notebookId && n.UserId == userId);

    public Task<bool> ExistsAsync(int notebookId, string userId) => context.Notebooks
        .AnyAsync(n => n.NotebookId == notebookId && n.UserId == userId);

    public async Task CreateAsync(Notebook notebook, string userId)
    {
        context.Notebooks.Add(new Notebook
        {
            NotebookName = notebook.NotebookName.Trim(),
            Description = notebook.Description?.Trim() ?? string.Empty,
            UserId = userId,
            DateCreated = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int notebookId, Notebook input, string userId)
    {
        var notebook = await context.Notebooks
            .SingleOrDefaultAsync(n => n.NotebookId == notebookId && n.UserId == userId);

        if (notebook is null)
            return false;

        notebook.NotebookName = input.NotebookName.Trim();
        notebook.Description = input.Description?.Trim() ?? string.Empty;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int notebookId, string userId)
    {
        var notebook = await context.Notebooks
            .SingleOrDefaultAsync(n => n.NotebookId == notebookId && n.UserId == userId);

        if (notebook is null)
            return false;

        // Deleting a notebook does not delete the notes inside it.
        // The optional Note.NotebookId foreign key is set to null by the database.
        context.Notebooks.Remove(notebook);
        await context.SaveChangesAsync();
        return true;
    }
}
