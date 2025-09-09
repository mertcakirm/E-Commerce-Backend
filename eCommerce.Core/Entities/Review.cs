namespace eCommerce.Core.Entities;

public class Review : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int Rating { get; set; } // 1-5
    public string Comment { get; set; }
}