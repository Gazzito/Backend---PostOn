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
    
    [Required]
    public bool IsOnline { get; set; }
    public Login Login { get; set; } // Navigation property

    // Navigation properties for friendships
    public ICollection<Friendship> Friendships { get; set; } = new List<Friendship>();

    // Navigation properties for friendships where the user is the friend
    public ICollection<Friendship> FriendshipsAsFriend { get; set; } = new List<Friendship>();

    public List<UserChat> UserChats { get; set; }
    public List<Message> Messages { get; set; }
}
