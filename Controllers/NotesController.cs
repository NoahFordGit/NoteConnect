using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteConnect.Models.Entities;
using NoteConnect.Services;

namespace NoteConnect.Controllers;

[Authorize]
public class NotesController(INoteRepository notes) : Controller
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
    public IActionResult Create() => View(new Note());

    // POST: Notes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NoteTitle,Content,NotebookId")] Note note)
    {
        SetOwnerAndValidate(note);

        if (!ModelState.IsValid)
            return View(note);

        await notes.CreateAsync(note, CurrentUserId);

        return RedirectToAction(nameof(Index));
    }

    // GET: Notes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var note = await FindNoteAsync(id);
        return note is null ? NotFound() : View(note);
    }

    // POST: Notes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("NoteTitle,Content,NotebookId")] Note input)
    {
        var note = await FindNoteAsync(id);

        if (note is null)
            return NotFound();

        SetOwnerAndValidate(input);

        if (!ModelState.IsValid)
        {
            input.NoteId = note.NoteId;
            input.DateCreated = note.DateCreated;
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

    private void SetOwnerAndValidate(Note note)
    {
        // Ownership comes from the login, never from submitted form fields.
        note.UserId = CurrentUserId;
        ModelState.Remove(nameof(Note.UserId));

        if (note.NotebookId <= 0)
            ModelState.AddModelError(nameof(Note.NotebookId), "Select a notebook.");
    }
}
