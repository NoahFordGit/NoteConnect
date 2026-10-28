using System.ComponentModel.DataAnnotations;

namespace NoteConnect.Models.Enities;

public class User {
    [Key]
    public int UserId {get; set;}
    [StringLength(128)]
    public string Username {get; set;} = String.Empty;
    [StringLength(128)]
    public string FirstName {get; set;}  = String.Empty;
    [StringLength(128)]
    public string LastName {get; set;} = String.Empty;
    [StringLength(256)]
    public string Email {get; set;} = String.Empty;
    [DataType(DataType.Date)]
    public DateTime DateCreated {get; set;}
} // End class