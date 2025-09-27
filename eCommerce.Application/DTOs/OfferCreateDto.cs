using Microsoft.AspNetCore.Http;

namespace eCommerce.Application.DTOs;

public class CreateOfferDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? DiscountRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Resim dosyası
    public IFormFile? ImageFile { get; set; }
}