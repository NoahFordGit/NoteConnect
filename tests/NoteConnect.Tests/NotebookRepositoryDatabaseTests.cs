using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteConnect.Data;
using NoteConnect.Models.Entities;
using NoteConnect.Services;

namespace NoteConnect.Tests;

[TestClass]
public class NotebookRepositoryDatabaseTests
{
    private static async Task<(SqliteConnection Connection, ApplicationDbContext Context)> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return (connection, context);
    }

    [TestMethod]
    public async Task GetAll_ReturnsOnlyNotebooksOwnedByUser()
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using var sqlite = connection;
        await using var db = context;
        db.Notebooks.AddRange(
            new Notebook { NotebookName = "School", UserId = "user-one" },
            new Notebook { NotebookName = "Private", UserId = "another-user" });
        await db.SaveChangesAsync();

        var notebooks = await new NotebookRepository(db).GetAllAsync("user-one");

        Assert.AreEqual(1, notebooks.Count);
        Assert.AreEqual("School", notebooks[0].NotebookName);
    }

    [TestMethod]
    public async Task Create_TrimsNotebookNameAndDescription()
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using var sqlite = connection;
        await using var db = context;
        var repository = new NotebookRepository(db);

        await repository.CreateAsync(new Notebook { NotebookName = "  School  ", Description = "  Class notes  " }, "user-one");

        var notebook = await db.Notebooks.SingleAsync();
        Assert.AreEqual("School", notebook.NotebookName);
        Assert.AreEqual("Class notes", notebook.Description);
        Assert.AreEqual("user-one", notebook.UserId);
    }

    [TestMethod]
    public async Task Update_DoesNotChangeOtherUsersNotebook()
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using var sqlite = connection;
        await using var db = context;
        var notebook = new Notebook { NotebookName = "Private", UserId = "another-user" };
        db.Notebooks.Add(notebook);
        await db.SaveChangesAsync();

        var result = await new NotebookRepository(db).UpdateAsync(notebook.NotebookId, new Notebook { NotebookName = "Changed" }, "user-one");

        Assert.IsFalse(result);
        Assert.AreEqual("Private", (await db.Notebooks.SingleAsync()).NotebookName);
    }

    [TestMethod]
    public async Task DeleteNotebook_PreservesNotesAsUnfiled()
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using var sqlite = connection;
        await using var db = context;
        var notebook = new Notebook { NotebookName = "School", UserId = "user-one" };
        db.Notebooks.Add(notebook);
        await db.SaveChangesAsync();
        db.Notes.Add(new Note { NoteTitle = "Chapter 1", UserId = "user-one", NotebookId = notebook.NotebookId });
        await db.SaveChangesAsync();

        var deleted = await new NotebookRepository(db).DeleteAsync(notebook.NotebookId, "user-one");
        db.ChangeTracker.Clear();
        var savedNote = await db.Notes.SingleAsync();

        Assert.IsTrue(deleted);
        Assert.AreEqual("Chapter 1", savedNote.NoteTitle);
        Assert.IsNull(savedNote.NotebookId);
        Assert.AreEqual(0, await db.Notebooks.CountAsync());
    }
}
