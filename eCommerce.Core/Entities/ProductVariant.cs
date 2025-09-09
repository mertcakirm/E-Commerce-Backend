namespace eCommerce.Core.Entities;

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }

    public virtual Product Product { get; set; }

    public string Size { get; set; }  
    public int Stock { get; set; }   
    public decimal CostPrice { get; set; }

    public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual List<CartItem> CartItems { get; set; } = new List<CartItem>();
}