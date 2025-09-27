using eCommerce.Application.Interfaces;
using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly IOfferService _offerService;

    public OffersController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    [HttpGet("{offerId}/products/discountmatch")]
    public async Task<IActionResult> GetDiscountMatchedProducts(int offerId)
    {
        var products = await _offerService.GetDiscountMatchedProductsAsync(offerId);
        return Ok(products.Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.DiscountRate
        }));
    }
}