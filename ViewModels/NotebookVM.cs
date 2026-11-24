using Microsoft.AspNetCore.Mvc.Rendering;
using NoteConnect.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NoteConnect.ViewModels
{
    public class NotebookViewModel
    {
         // Does not need an error message (db given or set value)
         public int NotebookId { get; set; }

        [Required(ErrorMessage = "Notebook name is required.")]
        public string NotebookName { get; set; } = string.Empty;

        // Description is not required 
        public string? Description { get; set; }

         // Does not need an error message (db given or set value)
        public DateTime DateGiven { get; set; } = DateTime.UtcNow;

         // Does not need an error message (db given or set value)
        public string UserId { get; set; } = string.Empty;

         // Does not need an error message (db given or set value)
        public List<NoteViewModel> Notes { get; set; } = new();

    } // End class
} // End namespace