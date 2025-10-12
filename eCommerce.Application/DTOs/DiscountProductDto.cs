using eCommerce.Core.Entities;

namespace eCommerce.Application.DTOs;

public class DiscountProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountRate { get; set; }
    public double AverageRating { get; set; }
    public decimal PriceWithDiscount { get; set; }
    public List<ProductImage> Images { get; set; } = new();
    public List<ProductVariantResponseDto> Variants { get; set; } = new();
}