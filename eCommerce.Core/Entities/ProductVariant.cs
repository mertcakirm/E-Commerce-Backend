namespace eCommerce.Core.Entities;

public class ProductVariant : BaseEntity
{

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public string Size { get; set; }  
    public int Stock { get; set; }   

    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
}