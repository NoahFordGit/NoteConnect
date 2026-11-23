using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteConnect.Models.Entities;
using NoteConnect.Services;
using NoteConnect.ViewModels;

namespace NoteConnect.Controllers;

[Authorize]
public class NotebooksController(INotebookRepository notebooks) : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;


    // Helper methods for ViewModel logic
    private static NotebookViewModel ToVM(Notebook notebook) => new()
    {
        NotebookId = notebook.NotebookId,
        NotebookName = notebook.NotebookName,
        Description = notebook.Description,
        DateGiven = notebook.DateCreated,
        UserId = notebook.UserId,
        Notes = notebook.Notes.Select(ToNoteVM).ToList()  
    };

    private static Notebook ToEntity(NotebookViewModel vm) => new()
    {
        NotebookId = vm.NotebookId,
        NotebookName = vm.NotebookName,
        Description = vm.Description,
        DateCreated = vm.DateGiven,
        UserId = vm.UserId,
    };

    private static NoteViewModel ToNoteVM(Note note) => new()
    {
        NoteId = note.NoteId,
        NoteTitle = note.NoteTitle,
        Content = note.Content,
        DateGiven = note.DateCreated,
        UserId = note.UserId,
        NotebookId = note.NotebookId, 
    };


    // GET: Notebooks
    public async Task<IActionResult> Index()
    {
        var allNotebooks = await notebooks.GetAllAsync(CurrentUserId);
        return View(allNotebooks.Select(ToVM));
    }

    // GET: Notebooks/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var notebook = await FindNotebookAsync(id);
        return notebook is null ? NotFound() : View(ToVM(notebook));
    }

    // GET: Notebooks/Create
    public IActionResult Create() => View(new NotebookViewModel());

    // POST: Notebooks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotebookViewModel input)
    {
        var notebook = ToEntity(input);
        SetOwnerAndValidate(notebook);

        if (!ModelState.IsValid)
            return View(notebook);

        await notebooks.CreateAsync(notebook, CurrentUserId);
        return RedirectToAction(nameof(Index));
    }

    // GET: Notebooks/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var notebook = await FindNotebookAsync(id);
        return notebook is null ? NotFound() : View(ToVM(notebook));
    }

    // POST: Notebooks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NotebookViewModel input)
    {
        var notebook = await FindNotebookAsync(id);

        if (notebook is null)
            return NotFound();


        var updated = ToEntity(input);
        updated.NotebookId = notebook.NotebookId;
        updated.DateCreated = notebook.DateCreated;

        SetOwnerAndValidate(updated);

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        if (!await notebooks.UpdateAsync(id, updated, CurrentUserId))
            return NotFound();

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Notebooks/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var notebook = await FindNotebookAsync(id);
        return notebook is null ? NotFound() : View(ToVM(notebook));
    }

    // POST: Notebooks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await notebooks.DeleteAsync(id, CurrentUserId))
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    private Task<Notebook?> FindNotebookAsync(int id) => notebooks.GetByIdAsync(id, CurrentUserId);

    private void SetOwnerAndValidate(Notebook notebook)
    {
        // Ownership comes from the login, never from submitted form fields.
        notebook.UserId = CurrentUserId;
        ModelState.Remove(nameof(Notebook.UserId));

        if (string.IsNullOrWhiteSpace(notebook.NotebookName))
            ModelState.AddModelError(nameof(Notebook.NotebookName), "Enter a notebook name.");
    }
}
