namespace eCommerce.Application.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DiscountRate { get; set; }
    public decimal BasePrice { get; set; }
    public double AverageRating { get; set; }
    public string CategoryName { get; set; }
    
    public decimal Price { get; set; }
    public int CategoryId { get; set; }

    public List<ProductVariantResponseDto> Variants { get; set; } = new();
    public List<ProductImageResponseDto> Images { get; set; } = new();
}

public class ProductVariantResponseDto
{
    public int Id { get; set; }
    public string Size { get; set; }
    public int Stock { get; set; }
}

public class ProductImageResponseDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public bool IsMain { get; set; }
}