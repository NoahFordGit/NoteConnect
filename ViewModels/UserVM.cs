using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NoteConnect.ViewModels
{
    public class UserVM {

        [Required(ErrorMessage = "First name is required.")]
        public string FirstNameSelected {get; set;} = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastNameSelected {get; set;} = string.Empty;

        public DateTime DateGiven {get; set;}

    } // End class
} // End namespace