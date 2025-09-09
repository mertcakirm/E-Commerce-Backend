namespace eCommerce.Core.Entities;

public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    public ICollection<CartItem> Items { get; set; }
}