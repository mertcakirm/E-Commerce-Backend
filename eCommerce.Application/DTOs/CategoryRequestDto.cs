using Microsoft.AspNetCore.Http;

namespace eCommerce.Application.DTOs;

public class CategoryRequestDto
{
    public string Name { get; set; }
    public IFormFile Image { get; set; }
}
