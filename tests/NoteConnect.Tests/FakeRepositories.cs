using NoteConnect.Models.Entities;
using NoteConnect.Services;

namespace NoteConnect.Tests;

public class FakeNotebookRepository : INotebookRepository
{
    public List<Notebook> Notebooks { get; } = new();
    public string? LastUserId { get; private set; }
    public Notebook? LastCreatedNotebook { get; private set; }
    public int CreateCalls { get; private set; }
    public int UpdateCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public Task<List<Notebook>> GetAllAsync(string userId)
    {
        LastUserId = userId;
        return Task.FromResult(Notebooks.Where(n => n.UserId == userId).ToList());
    }

    public Task<Notebook?> GetByIdAsync(int notebookId, string userId)
    {
        LastUserId = userId;
        return Task.FromResult(Notebooks.SingleOrDefault(n => n.NotebookId == notebookId && n.UserId == userId));
    }

    public Task<bool> ExistsAsync(int notebookId, string userId)
    {
        LastUserId = userId;
        return Task.FromResult(Notebooks.Any(n => n.NotebookId == notebookId && n.UserId == userId));
    }

    public Task CreateAsync(Notebook notebook, string userId)
    {
        LastUserId = userId;
        LastCreatedNotebook = notebook;
        CreateCalls++;
        Notebooks.Add(new Notebook
        {
            NotebookId = Notebooks.Count + 1,
            NotebookName = notebook.NotebookName,
            Description = notebook.Description,
            UserId = userId
        });
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(int notebookId, Notebook input, string userId)
    {
        LastUserId = userId;
        UpdateCalls++;
        var notebook = Notebooks.SingleOrDefault(n => n.NotebookId == notebookId && n.UserId == userId);
        if (notebook is null)
            return Task.FromResult(false);

        notebook.NotebookName = input.NotebookName;
        notebook.Description = input.Description;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int notebookId, string userId)
    {
        LastUserId = userId;
        DeleteCalls++;
        var notebook = Notebooks.SingleOrDefault(n => n.NotebookId == notebookId && n.UserId == userId);
        if (notebook is null)
            return Task.FromResult(false);

        Notebooks.Remove(notebook);
        return Task.FromResult(true);
    }
}

public class FakeNoteRepository : INoteRepository
{
    public List<Note> Notes { get; } = new();
    public Note? LastCreatedNote { get; private set; }
    public int CreateCalls { get; private set; }
    public int UpdateCalls { get; private set; }

    public Task<List<Note>> GetAllAsync(string userId) =>
        Task.FromResult(Notes.Where(n => n.UserId == userId).ToList());

    public Task<Note?> GetByIdAsync(int noteId, string userId) =>
        Task.FromResult(Notes.SingleOrDefault(n => n.NoteId == noteId && n.UserId == userId));

    public Task CreateAsync(Note note, string userId)
    {
        CreateCalls++;
        LastCreatedNote = note;
        Notes.Add(new Note
        {
            NoteId = Notes.Count + 1,
            NoteTitle = note.NoteTitle,
            NotebookId = note.NotebookId,
            UserId = userId
        });
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(int noteId, Note input, string userId)
    {
        UpdateCalls++;
        var note = Notes.SingleOrDefault(n => n.NoteId == noteId && n.UserId == userId);
        if (note is null)
            return Task.FromResult(false);

        note.NoteTitle = input.NoteTitle;
        note.NotebookId = input.NotebookId;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int noteId, string userId)
    {
        var note = Notes.SingleOrDefault(n => n.NoteId == noteId && n.UserId == userId);
        if (note is null)
            return Task.FromResult(false);

        Notes.Remove(note);
        return Task.FromResult(true);
    }
}
