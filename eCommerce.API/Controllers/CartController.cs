using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService  _cartService;
    public CartController(ICartService cartService)
        {
        _cartService = cartService;
        }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetCartAsync([FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var carts = await _cartService.GetUserCartAsync(token);
        return Ok(carts);
    }

    [Authorize]

    [HttpPut("increase/{cartItemId}")]
    public async Task<IActionResult> IncreaseCartAsync([FromHeader(Name = "Authorization")] string token,[FromQuery] int cartItemId)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var newCart = await _cartService.IncreaseItemAsync(cartItemId, token);
        return Ok(newCart);
    }
    
    [Authorize]

    [HttpPut("decrease/{cartItemId}")]
    public async Task<IActionResult> DecreaseCartAsync([FromHeader(Name = "Authorization")] string token,[FromQuery] int cartItemId)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var newCart = await _cartService.DecreaseItemAsync(cartItemId, token);
        return Ok(newCart);
    }

    [Authorize]
    [HttpDelete]

    public async Task<IActionResult> DeleteCartAsync([FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");
        var cart = await _cartService.ClearCartAsync(token);
        return Ok(cart);
    }
}