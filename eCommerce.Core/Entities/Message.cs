namespace eCommerce.Core.Entities;

public class Message : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public string MessageTitle { get; set; }
    public string MessageText { get; set; }
    public bool IsReply { get; set; }
    public string Answer {get; set;}
    
}