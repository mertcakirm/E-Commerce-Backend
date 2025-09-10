namespace eCommerce.Core.Entities;

public class Cart : BaseEntity
{
    public int UserId { get; set; }
    public virtual User User { get; set; }

    public virtual List<CartItem> CartItems { get; set; } = new List<CartItem>();
}