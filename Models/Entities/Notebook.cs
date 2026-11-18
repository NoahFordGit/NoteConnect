using System.ComponentModel.DataAnnotations;

namespace NoteConnect.Models.Entities;

public class Notebook
{
    public int NotebookId { get; set; }

    [Required]
    [StringLength(100)]
    public string NotebookName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
