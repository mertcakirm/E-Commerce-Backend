namespace eCommerce.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal Price { get; set; }
    public int DiscountRate { get; set; }
    public bool IsActive { get; set; } = true;
    public double AverageRating { get; set; } = 0.0;
    public int CategoryId { get; set; }
    public Category Category { get; set; }

    // Navigation properties
    public virtual List<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual List<Comment> Comments { get; set; } = new List<Comment>();
    public virtual List<ProductImage?> Images { get; set; } = new List<ProductImage?>();
    public virtual List<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual List<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public virtual List<Offer> ProductOffers { get; set; } = new();
}