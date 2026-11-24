using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NoteConnect.ViewModels
{
    public class UserViewModel {
        
        // Does not need an error message (db given or set value)
        public string? UserId {get; set;}

        [Required(ErrorMessage = "Username is required.")]
        public string? UserName {get; set;}

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName {get; set;} = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName {get; set;} = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        public string? Email {get; set;}

        [Required(ErrorMessage = "Phone number is required.")]
        public string? PhoneNumber {get; set;} 

        // Does not need an error message (db given or set value)
        public bool HasPassword {get; set;}
        
        // Does not need an error message (db given or set value)
        public DateTime DateGiven {get; set;}

    } // End class
} // End namespace