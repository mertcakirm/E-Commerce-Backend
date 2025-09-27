using Microsoft.AspNetCore.Http;

namespace eCommerce.Application.DTOs;

public class SliderContentDto
{
    public string ParentName { get; set; }
    public string Name { get; set; }
    public string SubName { get; set; }
    public string Href { get; set; }
    public IFormFile Image { get; set; }
}


public class CartContentDto
{
    public string Name { get; set; }
    public string Href { get; set; }
    public string CartSize { get; set; }
    public IFormFile Image { get; set; }
}

