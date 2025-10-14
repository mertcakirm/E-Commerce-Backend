namespace eCommerce.Application.DTOs;

using System.Collections.Generic;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DiscountRate { get; set; }
    public int SaleCount { get; set; } = 0;
    public double AverageRating { get; set; }

    // 🔹 Çoklu kategori
    public List<int> CategoryIds { get; set; } = new();
    public List<string> CategoryNames { get; set; } = new();

    public decimal Price { get; set; }
    public decimal PriceWithDiscount { get; set; }
    public bool IsDeleted { get; set; }

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