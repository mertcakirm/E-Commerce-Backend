namespace eCommerce.Core.Entities;

public class CartContent : BaseEntity
{
    public string Name { get; set; }
    public string Href { get; set; }
    public string CartSize { get; set; }
    public string ImageUrl { get; set; }
}