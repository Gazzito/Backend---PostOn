public class Message
{
    public int ID { get; set; }
    public int UserID { get; set; }
    public int ChatID { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }

    public User User { get; set; }
    public Chat Chat { get; set; }
}