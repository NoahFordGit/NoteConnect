using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NoteConnect.Models.Entities;
using NoteConnect.Services;
using NoteConnect.ViewModels;

namespace NoteConnect.Controllers;

[Authorize]
public class NotesController(INoteRepository notes, INotebookRepository notebooks) : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Mapped helpers for the ViewModel
    private static NoteViewModel ToVM(Note note) => new()
    {
        NoteId = note.NoteId,
        NoteTitle = note.NoteTitle,
        Content = note.Content,
        DateGiven = note.DateCreated,
        UserId = note.UserId,
        NotebookId = note.NotebookId,
        Notebook = note.Notebook  
    };


    private static Note ToEntity(NoteViewModel vm) => new()
    {
        NoteId = vm.NoteId,
        NoteTitle = vm.NoteTitle,
        Content = vm.Content,
        DateCreated = vm.DateGiven,
        UserId = vm.UserId,
        NotebookId = vm.NotebookId,
        Notebook = vm.Notebook
    };


    // GET: Notes
    public async Task<IActionResult> Index()
    {
        return View(await notes.GetAllAsync(CurrentUserId));
    }

    // GET: Notes/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var note = await FindNoteAsync(id);
        if (note is null)
        {
            return NotFound();
        }
        return View(ToVM(note));
    }

    // GET: Notes/Create
    public async Task<IActionResult> Create(int? notebookId)
    {
        var vm = new NoteViewModel();
        if (notebookId is int id && await notebooks.ExistsAsync(id, CurrentUserId))
            vm.NotebookId = id;

        await SetNotebookOptionsAsync(vm);
        return View(vm);
    }

    // POST: Notes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NoteViewModel input)
    {
        var note = ToEntity(input);
        await SetOwnerAndValidateAsync(note);

        if (!ModelState.IsValid)
        {
            await SetNotebookOptionsAsync(vm: input);
            return View(input);
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
        var vm = ToVM(note);
        await SetNotebookOptionsAsync(vm);
        return View(vm);
    }

    // POST: Notes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NoteViewModel input)
    {
        var note = await FindNoteAsync(id);

        if (note is null)
            return NotFound();

        var updated = ToEntity(input);
        updated.NoteId = note.NoteId;
        updated.DateCreated = note.DateCreated;

        await SetOwnerAndValidateAsync(updated);

        if (!ModelState.IsValid)
        {
            input.NoteId = note.NoteId;
            input.DateGiven = note.DateCreated;
            await SetNotebookOptionsAsync(input);
            return View(input);
        }

        if (!await notes.UpdateAsync(id, updated, CurrentUserId))
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // GET: Notes/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var note = await FindNoteAsync(id);
        return note is null ? NotFound() : View(ToVM(note));
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

    private async Task SetNotebookOptionsAsync(NoteViewModel vm)
    {
        vm.NotebookOptions = new SelectList(
            await notebooks.GetAllAsync(CurrentUserId),
            nameof(Notebook.NotebookId),
            nameof(Notebook.NotebookName),
            vm.NotebookId);
    }
}
