


public class Post
{
    public int Id { get; set; }
    public int CreatedBy { get; set; } 

    public required string Content { get; set; } 

    public required int Likes { get; set; } 

    public required int Dislikes { get; set; } 

    public DateTime CreatedOn { get; set; } // Timestamp of when the friendship was initially created
    public int UpdatedBy { get; set; } // User ID who initiated the last action
    public DateTime UpdatedOn { get; set; } // Timestamp of when the last action was performed

    public User User { get; set; }
}

