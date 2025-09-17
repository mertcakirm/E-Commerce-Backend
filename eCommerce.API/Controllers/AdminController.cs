using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    
    public AdminController(IProductService productService, ICategoryService categoryService, IOrderService orderService, IUserService userService)
        {
        _productService = productService;
        _categoryService = categoryService;
        _orderService = orderService;
        _userService = userService;
        }
    
    [Authorize]
    [HttpGet("users/get-all")]
    public async Task<IActionResult> GetAllUsers([FromHeader(Name = "Authorization")] string token,[FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var users = await _userService.GetAllUsers(token,pageNumber,pageSize);
        return Ok(users);
    }
    
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId,[FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _userService.DeleteUser(token, userId);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);
    }

    [HttpPut("{orderId}/payment")]
    [Authorize]
    public async Task<IActionResult> UpdatePaymentStatus([FromHeader(Name = "Authorization")] string token,int orderId, [FromBody] PaymentUpdateDto dto)
    {
        try
        {
            await _orderService.UpdatePaymentStatusAsync(orderId, dto.Status, token);
            return Ok(new { Success = true, Message = "Ödeme durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
        
        
    [HttpPut("{orderId}/order")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus([FromHeader(Name = "Authorization")] string token,int orderId, [FromBody] PaymentUpdateDto dto)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId, dto.Status, token);
            return Ok(new { Success = true, Message = "Sipariş durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
        
    [HttpPut("{orderId}/order/complete")]
    [Authorize]
    public async Task<IActionResult> CompleteOrderStatus([FromHeader(Name = "Authorization")] string token,int orderId)
    {
        try
        {
            await _orderService.CompleteOrderStatusAsync(orderId, token);
            return Ok(new { Success = true, Message = "Sipariş durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
    
    
    [Authorize]
    [HttpPost("product")]
    public async Task<IActionResult> CreateProduct([FromHeader(Name = "Authorization")] string token,[FromBody] ProductDto product)
    {
        var result = await _productService.CreateProductAsync(product,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return Created(result.UrlAsCreated!, result);
    }
    
    [Authorize]
    [HttpPut("products/{productId}/discount/{discountRate}")]
    public async Task<IActionResult> DiscountProduct([FromHeader(Name = "Authorization")] string token,int productId, int discountRate)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _productService.DiscountProduct(token, productId, discountRate);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);

    }
        
    [Authorize]
    [HttpPut("product/{id}")]
    public async Task<IActionResult> UpdateProduct([FromHeader(Name = "Authorization")] string token,int id, [FromBody] ProductDto product)
    {
        var result = await _productService.UpdateProductAsync(id, product,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("product/{id}")]
    public async Task<IActionResult> DeleteProduct([FromHeader(Name = "Authorization")] string token,int id)
    {
        var result = await _productService.DeleteProductAsync(id,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
        
    [Authorize]
    [HttpDelete("product/image/{id}")]
    public async Task<IActionResult> DeleteImage([FromHeader(Name = "Authorization")] string token,int id)
    {
        var result = await _productService.DeleteImageAsync(id,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
    
    [Authorize]
    [HttpPost("category")]
    public async Task<IActionResult> AddCategory([FromHeader(Name = "Authorization")] string token,[FromBody] CategoryRequestDto categoryDto)
    {
        var result = await _categoryService.AddCategoryAsync(categoryDto, token);

        if (result.IsFail) return StatusCode((int)result.Status, result.ErrorMessage);

        return Created(result.UrlAsCreated, result.Data);
    }
    
    [Authorize]
    [HttpDelete("category/{id}")]
    public async Task<IActionResult> DeleteCategory([FromHeader(Name = "Authorization")] string token,int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, token);

        if (result.IsFail) return StatusCode((int)result.Status, result.ErrorMessage);

        return NoContent();
    }
    
}