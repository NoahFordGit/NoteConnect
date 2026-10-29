using System.ComponentModel.DataAnnotations;

namespace NoteConnect.Models.Entities;

public class Note
{
    public int NoteId { get; set; }

    [Required]
    public string NoteTitle { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    // ASP.NET Identity uses string user IDs in this project.
    [Required]
    public string UserId { get; set; } = string.Empty;

    public int NotebookId { get; set; }
}
