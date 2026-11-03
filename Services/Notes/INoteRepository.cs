using NoteConnect.Models.Entities;

namespace NoteConnect.Services;

public interface INoteRepository
{
    Task<List<Note>> GetAllAsync(string userId);
    Task<Note?> GetByIdAsync(int noteId, string userId);
    Task CreateAsync(Note note, string userId);
    Task<bool> UpdateAsync(int noteId, Note input, string userId);
    Task<bool> DeleteAsync(int noteId, string userId);
}
