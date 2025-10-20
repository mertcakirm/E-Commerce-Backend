using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SliderCartController : ControllerBase
    {
        private readonly ISliderCartService _service;
        private readonly IWebHostEnvironment _env;

        public SliderCartController(ISliderCartService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }


        [AllowAnonymous]
        [HttpGet("sliders")]
        public async Task<IActionResult> GetAllSliders()
        {
            var result = await _service.GetAllSlidersAsync();
            return Ok(result);
        }

        [HttpGet("sliders/{id}")]
        public async Task<IActionResult> GetSliderById(int id)
        {
            var result = await _service.GetSliderByIdAsync(id);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpPost("sliders")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> AddSlider(
            [FromForm] SliderContentDto dto,
            [FromHeader(Name = "Authorization")] string token)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest("Resim dosyası gerekli!");

            var uploads = Path.Combine(_env.WebRootPath, "contents");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
            var filePath = Path.Combine(uploads, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            var slider = new SliderContent
            {
                ImageUrl = $"/contents/{fileName}",
                ParentName = dto.ParentName,
                Name = dto.Name,
                SubName = dto.SubName,
                Href = dto.Href
            };

            var result = await _service.AddSliderAsync(slider, token);
            if (!result.IsSuccess) return StatusCode((int)result.Status, result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpDelete("sliders/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSlider(int id, [FromHeader(Name = "Authorization")] string token)
        {
            var result = await _service.DeleteSliderAsync(id, token);
            if (!result.IsSuccess) return StatusCode((int)result.Status, result.ErrorMessage);
            return Ok(new { message = "Slider silindi." });
        }



        [AllowAnonymous]
        [HttpGet("carts")]
        public async Task<IActionResult> GetAllCarts()
        {
            var result = await _service.GetAllCartsAsync();
            return Ok(result);
        }

        [HttpGet("carts/{id}")]
        public async Task<IActionResult> GetCartById(int id)
        {
            var result = await _service.GetCartByIdAsync(id);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }



        [HttpPost("carts")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<IActionResult> AddCart(
            [FromForm] CartContentDto dto,
            [FromHeader(Name = "Authorization")] string token)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest("Resim dosyası gerekli!");

            var uploads = Path.Combine(_env.WebRootPath, "contents");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}_{dto.Image.FileName}";
            var filePath = Path.Combine(uploads, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            var cart = new CartContent
            {
                Name = dto.Name,
                CartSize = dto.CartSize,
                Href = dto.Href,
                ImageUrl = $"/contents/{fileName}"
            };

            var result = await _service.AddCartAsync(cart, token);
            if (!result.IsSuccess) return StatusCode((int)result.Status, result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpDelete("carts/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCart(int id, [FromHeader(Name = "Authorization")] string token)
        {
            var result = await _service.DeleteCartAsync(id, token);
            if (!result.IsSuccess) return StatusCode((int)result.Status, result.ErrorMessage);
            return Ok(new { message = "Cart silindi." });
        }

    }
}