using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoteConnect.Models.Entities;
using NoteConnect.Services;

namespace NoteConnect.Controllers;

[Authorize]
public class NotesController(INoteRepository notes, INotebookRepository notebooks) : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: Notes
    public async Task<IActionResult> Index()
    {
        return View(await notes.GetAllAsync(CurrentUserId));
    }

    // GET: Notes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var note = await FindNoteAsync(id);
        return note is null ? NotFound() : View(note);
    }

    // GET: Notes/Create
    public async Task<IActionResult> Create(int? notebookId)
    {
        var note = new Note();
        if (notebookId is int id && await notebooks.ExistsAsync(id, CurrentUserId))
            note.NotebookId = id;

        await SetNotebookOptionsAsync(note.NotebookId);
        return View(note);
    }

    // POST: Notes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NoteTitle,Content,NotebookId")] Note note)
    {
        await SetOwnerAndValidateAsync(note);

        if (!ModelState.IsValid)
        {
            await SetNotebookOptionsAsync(note.NotebookId);
            return View(note);
        }

        await notes.CreateAsync(note, CurrentUserId);
        return RedirectToAction(nameof(Index));
    }

    // GET: Notes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var note = await FindNoteAsync(id);
        if (note is null)
            return NotFound();

        await SetNotebookOptionsAsync(note.NotebookId);
        return View(note);
    }

    // POST: Notes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("NoteTitle,Content,NotebookId")] Note input)
    {
        var note = await FindNoteAsync(id);

        if (note is null)
            return NotFound();

        await SetOwnerAndValidateAsync(input);

        if (!ModelState.IsValid)
        {
            input.NoteId = note.NoteId;
            input.DateCreated = note.DateCreated;
            await SetNotebookOptionsAsync(input.NotebookId);
            return View(input);
        }

        if (!await notes.UpdateAsync(id, input, CurrentUserId))
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // GET: Notes/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var note = await FindNoteAsync(id);
        return note is null ? NotFound() : View(note);
    }

    // POST: Notes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await notes.DeleteAsync(id, CurrentUserId))
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    private Task<Note?> FindNoteAsync(int id) => notes.GetByIdAsync(id, CurrentUserId);

    private async Task SetOwnerAndValidateAsync(Note note)
    {
        // Ownership comes from the login, never from submitted form fields.
        note.UserId = CurrentUserId;
        ModelState.Remove(nameof(Note.UserId));

        // A note can be unfiled, or stored in a notebook owned by this user.
        if (note.NotebookId is int notebookId &&
            (notebookId <= 0 || !await notebooks.ExistsAsync(notebookId, CurrentUserId)))
        {
            ModelState.AddModelError(nameof(Note.NotebookId), "Select one of your notebooks.");
        }
    }

    private async Task SetNotebookOptionsAsync(int? notebookId)
    {
        ViewBag.Notebooks = new SelectList(
            await notebooks.GetAllAsync(CurrentUserId),
            nameof(Notebook.NotebookId),
            nameof(Notebook.NotebookName),
            notebookId);
    }
}
