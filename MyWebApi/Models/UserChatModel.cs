public class UserChat
{
    public int ID { get; set; }
    public int UserID { get; set; }
    public int ChatID { get; set; }
    
    public Chat Chat { get; set; }
    public User User { get; set; }
}
