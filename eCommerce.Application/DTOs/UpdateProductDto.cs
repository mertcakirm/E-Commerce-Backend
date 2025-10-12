namespace eCommerce.Application.DTOs;

using System.Collections.Generic;

public class UpdateProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal Price { get; set; }

    public List<int> CategoryIds { get; set; } = new();
}