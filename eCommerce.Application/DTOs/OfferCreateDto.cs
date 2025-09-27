namespace eCommerce.Application.DTOs;

public class OfferCreateDto
{
    public string Name { get; set; }
    public string ImageUrl { get; set; }
    public string Description { get; set; }
    public decimal? DiscountRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<int> ProductIds { get; set; }
}