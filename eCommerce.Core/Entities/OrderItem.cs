namespace eCommerce.Core.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; }

    public int Quantity { get; set; }
    public decimal Price { get; set; }
}