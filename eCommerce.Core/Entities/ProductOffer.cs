namespace eCommerce.Core.Entities;

public class ProductOffer
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int OfferId { get; set; }
    public Offer Offer { get; set; }
}