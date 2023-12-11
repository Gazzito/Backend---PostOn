


public class Friendship
{
    public int Id { get; set; }
    public int FriendId { get; set; }
    public int CreatedBy { get; set; } // User ID who initiated the initial friendship
    public DateTime CreatedOn { get; set; } // Timestamp of when the friendship was initially created
    public int UpdatedBy { get; set; } // User ID who initiated the last action
    public DateTime UpdatedOn { get; set; } // Timestamp of when the last action was performed

    public FriendState State { get; set; }
    // Navigation properties
    public User User { get; set; }
}


public enum FriendState
{
    Pending,
    Accepted,
    Rejected
}
