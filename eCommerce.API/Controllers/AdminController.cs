using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _env;

    public AdminController(IProductService productService, IWebHostEnvironment env)
    {
        _productService = productService;
        _env = env;
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
}