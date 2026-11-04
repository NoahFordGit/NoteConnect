using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NoteConnect.Models.Entities;
using NoteConnect.Services;

namespace NoteConnect.Controllers;

[Authorize]
public class NotebooksController(INotebookRepository notebooks) : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: Notebooks
    public async Task<IActionResult> Index()
    {
        return View(await notebooks.GetAllAsync(CurrentUserId));
    }

    // GET: Notebooks/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var notebook = await FindNotebookAsync(id);
        return notebook is null ? NotFound() : View(notebook);
    }

    // GET: Notebooks/Create
    public IActionResult Create() => View(new Notebook());

    // POST: Notebooks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NotebookName,Description")] Notebook notebook)
    {
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
        return notebook is null ? NotFound() : View(notebook);
    }

    // POST: Notebooks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("NotebookName,Description")] Notebook input)
    {
        var notebook = await FindNotebookAsync(id);

        if (notebook is null)
            return NotFound();

        SetOwnerAndValidate(input);

        if (!ModelState.IsValid)
        {
            input.NotebookId = notebook.NotebookId;
            input.DateCreated = notebook.DateCreated;
            return View(input);
        }

        if (!await notebooks.UpdateAsync(id, input, CurrentUserId))
            return NotFound();

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Notebooks/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var notebook = await FindNotebookAsync(id);
        return notebook is null ? NotFound() : View(notebook);
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
