using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace NoteConnect.Models.Entities;

// AspNetUser table has everything we need, this class just adds the extra fields
public class User : IdentityUser {
    public string FirstName {get; set;}  = String.Empty;
    [StringLength(128)]
    public string LastName {get; set;} = String.Empty;

    // Changed date so that the date is set automatically
    [BindNever]
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
} // End class