using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteConnect.Models.Entities;

namespace NoteConnect.Tests;

[TestClass]
public class NotebookModelTests
{
    private static List<ValidationResult> Validate(Notebook notebook)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(notebook, new ValidationContext(notebook), errors, true);
        return errors;
    }

    [TestMethod]
    public void ValidNotebook_HasNoValidationErrors()
    {
        var notebook = new Notebook { NotebookName = "School", Description = "Class notes", UserId = "user-one" };

        var errors = Validate(notebook);

        Assert.AreEqual(0, errors.Count);
    }

    [TestMethod]
    public void EmptyName_FailsValidation()
    {
        var notebook = new Notebook { NotebookName = "", UserId = "user-one" };

        var errors = Validate(notebook);

        Assert.IsTrue(errors.Any(e => e.MemberNames.Contains(nameof(Notebook.NotebookName))));
    }

    [TestMethod]
    public void NameLongerThan100Characters_FailsValidation()
    {
        var notebook = new Notebook { NotebookName = new string('A', 101), UserId = "user-one" };

        var errors = Validate(notebook);

        Assert.IsTrue(errors.Any(e => e.MemberNames.Contains(nameof(Notebook.NotebookName))));
    }

    [TestMethod]
    public void DescriptionLongerThan500Characters_FailsValidation()
    {
        var notebook = new Notebook { NotebookName = "School", Description = new string('A', 501), UserId = "user-one" };

        var errors = Validate(notebook);

        Assert.IsTrue(errors.Any(e => e.MemberNames.Contains(nameof(Notebook.Description))));
    }
}
