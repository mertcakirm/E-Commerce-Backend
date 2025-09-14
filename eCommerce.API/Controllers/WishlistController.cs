using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserWishlist([FromHeader(Name = "Authorization")] string token,[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _wishlistService.GetUserWishlistAsync(token, pageNumber, pageSize);

            return StatusCode((int)result.Status, result);
        }

        [Authorize]
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToWishlist([FromHeader(Name = "Authorization")] string token,int productId)
        {
            var result = await _wishlistService.AddToWishlistAsync(productId, token);

            return StatusCode((int)result.Status, result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromWishlist([FromHeader(Name = "Authorization")] string token,int id)
        {
            var result = await _wishlistService.RemoveFromWishlistAsync(id, token);

            return StatusCode((int)result.Status, result);
        }
    }
}