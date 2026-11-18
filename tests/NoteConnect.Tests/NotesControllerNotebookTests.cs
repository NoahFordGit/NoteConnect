using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteConnect.Controllers;
using NoteConnect.Models.Entities;

namespace NoteConnect.Tests;

[TestClass]
public class NotesControllerNotebookTests
{
    private const string UserId = "user-one";

    private static NotesController CreateController(FakeNoteRepository notes, FakeNotebookRepository notebooks)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId)
        }, "TestAuthentication");

        return new NotesController(notes, notebooks)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    [TestMethod]
    public async Task CreatePage_OwnedNotebook_PreselectsNotebook()
    {
        var notes = new FakeNoteRepository();
        var notebooks = new FakeNotebookRepository();
        notebooks.Notebooks.Add(new Notebook { NotebookId = 3, NotebookName = "School", UserId = UserId });
        var controller = CreateController(notes, notebooks);

        var result = await controller.Create(3);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var note = ((ViewResult)result).Model as Note;
        Assert.IsNotNull(note);
        Assert.AreEqual(3, note.NotebookId);
    }

    [TestMethod]
    public async Task CreatePage_OtherUsersNotebook_DoesNotPreselectIt()
    {
        var notes = new FakeNoteRepository();
        var notebooks = new FakeNotebookRepository();
        notebooks.Notebooks.Add(new Notebook { NotebookId = 3, NotebookName = "Private", UserId = "another-user" });
        var controller = CreateController(notes, notebooks);

        var result = await controller.Create(3);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var note = ((ViewResult)result).Model as Note;
        Assert.IsNotNull(note);
        Assert.IsNull(note.NotebookId);
    }

    [TestMethod]
    public async Task CreateNote_ValidNotebook_SavesNote()
    {
        var notes = new FakeNoteRepository();
        var notebooks = new FakeNotebookRepository();
        notebooks.Notebooks.Add(new Notebook { NotebookId = 3, NotebookName = "School", UserId = UserId });
        var controller = CreateController(notes, notebooks);

        var result = await controller.Create(new Note { NoteTitle = "Exam review", NotebookId = 3, UserId = "spoofed-user" });

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual(1, notes.CreateCalls);
        Assert.AreEqual(3, notes.LastCreatedNote!.NotebookId);
        Assert.AreEqual(UserId, notes.LastCreatedNote.UserId);
    }

    [TestMethod]
    public async Task CreateNote_OtherUsersNotebook_RejectsAssignment()
    {
        var notes = new FakeNoteRepository();
        var notebooks = new FakeNotebookRepository();
        notebooks.Notebooks.Add(new Notebook { NotebookId = 3, NotebookName = "Private", UserId = "another-user" });
        var controller = CreateController(notes, notebooks);

        var result = await controller.Create(new Note { NoteTitle = "Exam review", NotebookId = 3 });

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.AreEqual(0, notes.CreateCalls);
    }

    [TestMethod]
    public async Task CreateNote_UnfiledNote_IsAllowed()
    {
        var notes = new FakeNoteRepository();
        var notebooks = new FakeNotebookRepository();
        var controller = CreateController(notes, notebooks);

        var result = await controller.Create(new Note { NoteTitle = "Unfiled", NotebookId = null });

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual(1, notes.CreateCalls);
        Assert.IsNull(notes.LastCreatedNote!.NotebookId);
    }

    [TestMethod]
    public async Task EditNote_OtherUsersNotebook_RejectsMove()
    {
        var notes = new FakeNoteRepository();
        notes.Notes.Add(new Note { NoteId = 4, NoteTitle = "Existing", UserId = UserId });
        var notebooks = new FakeNotebookRepository();
        notebooks.Notebooks.Add(new Notebook { NotebookId = 7, NotebookName = "Private", UserId = "another-user" });
        var controller = CreateController(notes, notebooks);

        var result = await controller.Edit(4, new Note { NoteTitle = "Existing", NotebookId = 7 });

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.AreEqual(0, notes.UpdateCalls);
        Assert.IsNull(notes.Notes[0].NotebookId);
    }
}
