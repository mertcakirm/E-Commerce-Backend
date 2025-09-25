using Microsoft.AspNetCore.Http;

public class ProductCreateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }

    public List<ProductVariantCreateDto> Variants { get; set; } = new();

    public List<IFormFile> Images { get; set; } = new();
}

public class ProductVariantCreateDto
{
    public string Size { get; set; }
    public int Stock { get; set; }
}