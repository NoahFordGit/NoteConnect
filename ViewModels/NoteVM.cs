using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace NoteConnect.ViewModels
{
    public class NoteVM
    {
        public int IdGiven {get; set;}

        [Required(ErrorMessage = "Note title is required.")]
        public string TitleSelected {get; set;} = string.Empty;

        [Required(ErrorMessage = "Note content cannot be empty.")]
        public string ContentSelected {get; set;} = string.Empty;

        public DateTime DateGiven {get; set;}


    } // End class
} // End namespace