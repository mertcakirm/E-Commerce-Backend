using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IAuthRepository  _authRepository;
    private readonly ITokenService _tokenService;

    public AdminController(IProductService productService, IAuthRepository authRepository, ITokenService tokenService)
    {
        _productService = productService;
        _authRepository = authRepository;
        _tokenService = tokenService;
    }
    
    [HttpGet("products")]
    [Authorize]
    public async Task<IActionResult> GetAllAdmin(
        [FromHeader(Name = "Authorization")] string token,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        
        if (token == null) return StatusCode(401);

        var result = await _productService.GetAllProductsAdminAsync(pageNumber, pageSize,token);
        if (result.IsFail) return StatusCode((int)result.Status, result);

        return Ok(result);
    }
    
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Email ve şifre boş olamaz.");

        var adminUser = await _authRepository.LoginAdmin(request.Email, request.Password);
        if (adminUser == null)
            return Unauthorized("Geçersiz e-posta veya şifre.");

        var tokenString = _tokenService.CreateToken(adminUser);

        return Ok(new
        {
            message = "Giriş başarılı.",
            user = new
            {
                adminUser.Id,
                adminUser.Email,
                adminUser.Role
            },
            tokenString
        });
    }
    
    [HttpGet("low-stock")]
    [Authorize]
    public async Task<IActionResult> GetLowStockProducts([FromHeader(Name = "Authorization")] string token,[FromQuery] int limit = 20)
    {
        var result = await _productService.GetLowStockProductsAsync(limit, token);

        if (result.IsFail)
        {
            return StatusCode((int)result.Status, new { message = result.ErrorMessage });
        }

        return Ok(result.Data);
    }
}
