using Microsoft.AspNetCore.Mvc.Rendering;
using NoteConnect.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NoteConnect.ViewModels
{
    public class NoteViewModel
    {
        // Does not need an error message (db given or set value)        
        public int NoteId {get; set;}

        [Required(ErrorMessage = "Note title is required.")]
        public string NoteTitle {get; set;} = string.Empty;

        [Required(ErrorMessage = "Note content cannot be empty.")]
        public string Content {get; set;} = string.Empty;

        // Does not need an error message (db given or set value)
        public DateTime DateGiven {get; set;}

        // FK properties
        public string? UserId {get; set;}

        public int? NotebookId {get; set;}

        public Notebook? Notebook {get; set;}

        // May not need this
        public SelectList? NotebookOptions {get; set;}


    } // End class
} // End namespace