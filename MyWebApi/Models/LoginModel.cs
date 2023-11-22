// User.cs (Model)
public class Login
{
    public int Id { get; set; }

    // Other login properties...

    public int UserId { get; set; } // Foreign key
    public User User { get; set; } // Navigation property
}