using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
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
    
    [HttpPost]
    [Authorize]
    [RequestSizeLimit(10_000_000)] // 10 MB max
    public async Task<IActionResult> CreateOffer([FromHeader(Name = "Authorization")] string token,[FromForm] CreateOfferDto dto)
    {
        if (dto.StartDate > dto.EndDate)
            return BadRequest("StartDate cannot be after EndDate.");

        var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var offer = await _offerService.CreateOfferAsync(dto, wwwRootPath,token);
        return Ok(offer);
    }
    
    [HttpGet("all")]
    public async Task<IActionResult> GetOffersAll()
    {
        var offers = await _offerService.GetAllAsync();
        return Ok(offers);
    }

    [HttpGet("{offerId}/products/discountmatch")]
    public async Task<IActionResult> GetDiscountMatchedProducts(int offerId, int pageNumber = 1, int pageSize = 12)
    {
        var products = await _offerService.GetDiscountMatchedProductsAsync(offerId, pageNumber, pageSize);
        return Ok(products);
    }

    [HttpDelete("{offerId}")]
    [Authorize]
    public async Task<IActionResult> DeleteOffer([FromHeader(Name = "Authorization")] string token,int offerId)
    {
        var offer = await _offerService.DeleteOfferAsync(offerId,token);
        return Ok(offer);
    }
    [HttpPut("toggle/{offerId}")]
    [Authorize]
    public async Task<IActionResult> ToggleOffer([FromHeader(Name = "Authorization")] string token,int offerId)
    {
        var offer = await _offerService.ToggleOfferAsync(offerId,token);
        return Ok(offer);
    }
    
}