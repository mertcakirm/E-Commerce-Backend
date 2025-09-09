namespace eCommerce.Core.Entities;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart Cart { get; set; }

    public int ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; }

    public int Quantity { get; set; }
}