using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteConnect.Controllers;
using NoteConnect.Models.Entities;

namespace NoteConnect.Tests;

[TestClass]
public class NotebooksControllerTests
{
    private const string UserId = "user-one";

    private static NotebooksController CreateController(FakeNotebookRepository repository)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId)
        }, "TestAuthentication");

        return new NotebooksController(repository)
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
    public void Controller_RequiresAuthentication()
    {
        var attribute = Attribute.GetCustomAttribute(typeof(NotebooksController), typeof(AuthorizeAttribute));
        Assert.IsNotNull(attribute);
    }

    [TestMethod]
    public async Task Index_ReturnsOnlySignedInUsersNotebooks()
    {
        var repository = new FakeNotebookRepository();
        repository.Notebooks.Add(new Notebook { NotebookId = 1, NotebookName = "School", UserId = UserId });
        repository.Notebooks.Add(new Notebook { NotebookId = 2, NotebookName = "Private", UserId = "another-user" });
        var controller = CreateController(repository);

        var result = await controller.Index();

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        var notebooks = ((ViewResult)result).Model as List<Notebook>;
        Assert.IsNotNull(notebooks);
        Assert.AreEqual(1, notebooks.Count);
        Assert.AreEqual("School", notebooks[0].NotebookName);
        Assert.AreEqual(UserId, repository.LastUserId);
    }

    [TestMethod]
    public async Task Details_ExistingOwnedNotebook_ReturnsView()
    {
        var repository = new FakeNotebookRepository();
        repository.Notebooks.Add(new Notebook { NotebookId = 5, NotebookName = "Classes", UserId = UserId });
        var controller = CreateController(repository);

        var result = await controller.Details(5);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.AreEqual(5, ((Notebook)((ViewResult)result).Model!).NotebookId);
    }

    [TestMethod]
    public async Task Details_OtherUsersNotebook_ReturnsNotFound()
    {
        var repository = new FakeNotebookRepository();
        repository.Notebooks.Add(new Notebook { NotebookId = 5, NotebookName = "Private", UserId = "another-user" });
        var controller = CreateController(repository);

        var result = await controller.Details(5);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Create_ValidInput_UsesSignedInOwner()
    {
        var repository = new FakeNotebookRepository();
        var controller = CreateController(repository);
        var input = new Notebook { NotebookName = "School", Description = "Course notes", UserId = "spoofed-user" };

        var result = await controller.Create(input);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual("Index", ((RedirectToActionResult)result).ActionName);
        Assert.AreEqual(1, repository.CreateCalls);
        Assert.AreEqual(UserId, repository.LastUserId);
        Assert.AreEqual(UserId, repository.LastCreatedNotebook!.UserId);
    }

    [TestMethod]
    public async Task Create_WhitespaceName_ReturnsValidationError()
    {
        var repository = new FakeNotebookRepository();
        var controller = CreateController(repository);

        var result = await controller.Create(new Notebook { NotebookName = "   " });

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.AreEqual(0, repository.CreateCalls);
    }

    [TestMethod]
    public async Task Edit_ValidInput_UpdatesOwnedNotebook()
    {
        var repository = new FakeNotebookRepository();
        repository.Notebooks.Add(new Notebook { NotebookId = 8, NotebookName = "Old", UserId = UserId });
        var controller = CreateController(repository);

        var result = await controller.Edit(8, new Notebook { NotebookName = "New", Description = "Updated" });

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual("Details", ((RedirectToActionResult)result).ActionName);
        Assert.AreEqual("New", repository.Notebooks[0].NotebookName);
        Assert.AreEqual("Updated", repository.Notebooks[0].Description);
        Assert.AreEqual(1, repository.UpdateCalls);
    }

    [TestMethod]
    public async Task Edit_OtherUsersNotebook_DoesNotUpdate()
    {
        var repository = new FakeNotebookRepository();
        repository.Notebooks.Add(new Notebook { NotebookId = 8, NotebookName = "Private", UserId = "another-user" });
        var controller = CreateController(repository);

        var result = await controller.Edit(8, new Notebook { NotebookName = "Changed" });

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        Assert.AreEqual(0, repository.UpdateCalls);
        Assert.AreEqual("Private", repository.Notebooks[0].NotebookName);
    }

    [TestMethod]
    public async Task Delete_ExistingNotebook_RemovesIt()
    {
        var repository = new FakeNotebookRepository();
        repository.Notebooks.Add(new Notebook { NotebookId = 9, NotebookName = "Old", UserId = UserId });
        var controller = CreateController(repository);

        var result = await controller.DeleteConfirmed(9);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual("Index", ((RedirectToActionResult)result).ActionName);
        Assert.AreEqual(0, repository.Notebooks.Count);
    }

    [TestMethod]
    public async Task Delete_OtherUsersNotebook_ReturnsNotFound()
    {
        var repository = new FakeNotebookRepository();
        repository.Notebooks.Add(new Notebook { NotebookId = 9, NotebookName = "Private", UserId = "another-user" });
        var controller = CreateController(repository);

        var result = await controller.DeleteConfirmed(9);

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        Assert.AreEqual(1, repository.Notebooks.Count);
    }
}
