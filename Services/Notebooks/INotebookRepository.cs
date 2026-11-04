using NoteConnect.Models.Entities;

namespace NoteConnect.Services;

public interface INotebookRepository
{
    Task<List<Notebook>> GetAllAsync(string userId);
    Task<Notebook?> GetByIdAsync(int notebookId, string userId);
    Task<bool> ExistsAsync(int notebookId, string userId);
    Task CreateAsync(Notebook notebook, string userId);
    Task<bool> UpdateAsync(int notebookId, Notebook input, string userId);
    Task<bool> DeleteAsync(int notebookId, string userId);
}
