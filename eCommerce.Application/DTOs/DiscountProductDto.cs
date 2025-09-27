namespace eCommerce.Application.DTOs;

public class DiscountProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountRate { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<ProductVariantResponseDto> Variants { get; set; } = new();
}