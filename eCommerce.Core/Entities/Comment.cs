namespace eCommerce.Core.Entities;

public class Comment : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string CommentText { get; set; }

    public User User { get; set; }
    public Product Product { get; set; }
}