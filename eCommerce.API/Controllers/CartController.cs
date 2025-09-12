using eCommerce.Application;
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
    [HttpPost("add")]
    public async Task<IActionResult> AddItem(int productId, int productVariantId)
    {
        var token = Request.Headers["Authorization"].ToString();
        var result = await _cartService.AddItemAsync(token, productId, productVariantId);

        if (result.IsFail)
            return StatusCode((int)result.Status, result.ErrorMessage);

        return StatusCode((int)result.Status);
    }

    [Authorize]
    [HttpPut("increase")]
    public async Task<IActionResult> IncreaseCartAsync([FromHeader(Name = "Authorization")] string token, [FromQuery] int productId)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var result = await _cartService.IncreaseItemAsync(productId, token);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);
    }

    [Authorize]
    [HttpPut("decrease")]
    public async Task<IActionResult> DecreaseCartAsync([FromHeader(Name = "Authorization")] string token, [FromQuery] int productId)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var result = await _cartService.DecreaseItemAsync(productId, token);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);
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