public class Chat
{
    public int ID { get; set; }
    public string Name { get; set; }
    
    public ICollection<UserChat> UserChats { get; set; }
    public ICollection<Message> Messages { get; set; }
}