// User.cs (Model)
using System.ComponentModel.DataAnnotations;

public class Login
{
    public int Id { get; set; }

    // Other login properties...

    public int UserId { get; set; } // Foreign key

    [Required]
    public string Username { get; set; }

    public string PasswordHash { get; set; }
    public string Salt { get; set; }

    public Role Role { get; set; }

    public User User { get; set; } // Navigation property
}

public enum Role
{
    Guest,
    Registed,
    Admin
}
