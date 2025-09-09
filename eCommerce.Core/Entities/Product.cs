namespace eCommerce.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    public ICollection<ProductImage> Images { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<Wishlist> Wishlists { get; set; }
    public ICollection<Review> Reviews { get; set; }
}