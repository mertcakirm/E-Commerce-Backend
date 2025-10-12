namespace eCommerce.Application.DTOs;

public class ProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal Price { get; set; }

    public List<int> CategoryIds { get; set; } = new();
    public List<string> CategoryNames { get; set; } = new();

    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductVariantDto
{
    public string Size { get; set; }
    public int Stock { get; set; }
}

public class ProductImageDto
{
    public string ImageUrl { get; set; }
    public bool IsMain { get; set; }
}