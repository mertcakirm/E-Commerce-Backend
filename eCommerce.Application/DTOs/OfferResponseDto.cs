namespace eCommerce.Application.DTOs;

public class OfferResponseDto
{
    public int Id { get; set; }
    public string OfferName {get; set;}
    
    public List<ProductResponseDto> Products { get; set; } = new();
    
}