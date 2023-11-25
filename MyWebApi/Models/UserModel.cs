// User.cs (Model)

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int UserId { get; set; }

    [Required]
    public string FirstName { get; set; }

     [Required]
    public string LastName { get; set; }

    [Required]
    public string Email { get; set; }

    public string Biography { get; set; }

    public string ProfilePic { get; set; }

    public Login Login { get; set; } // Navigation property
}
