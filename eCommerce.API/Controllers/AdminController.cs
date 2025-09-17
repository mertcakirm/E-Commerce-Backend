using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    
    public AdminController(IAdminService adminService)
        {
        _adminService = adminService;
        }
    
    [Authorize]
    [HttpGet("users/get-all")]
    public async Task<IActionResult> GetAllUsers([FromHeader(Name = "Authorization")] string token,[FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var users = await _adminService.GetAllUsers(token,pageNumber,pageSize);
        return Ok(users);
    }
    
    [Authorize]
    [HttpPut("products/{productId}/discount/{discountRate}")]
    public async Task<IActionResult> DiscountProduct([FromHeader(Name = "Authorization")] string token,int productId, int discountRate)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _adminService.DiscountProduct(token, productId, discountRate);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);

    }
    
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId,[FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _adminService.DeleteUser(token, userId);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);
    }

    // PUT: api/Orders/{orderId}/payment
    [HttpPut("{orderId}/payment")]
    [Authorize]
    public async Task<IActionResult> UpdatePaymentStatus([FromHeader(Name = "Authorization")] string token,int orderId, [FromBody] PaymentUpdateDto dto)
    {
        try
        {
            await _adminService.UpdatePaymentStatusAsync(orderId, dto.Status, token);
            return Ok(new { Success = true, Message = "Ödeme durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
        
        
    // PUT: api/Orders/{orderId}/payment
    [HttpPut("{orderId}/order")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus([FromHeader(Name = "Authorization")] string token,int orderId, [FromBody] PaymentUpdateDto dto)
    {
        try
        {
            await _adminService.UpdateOrderStatusAsync(orderId, dto.Status, token);
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
            await _adminService.CompleteOrderStatusAsync(orderId, token);
            return Ok(new { Success = true, Message = "Sipariş durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
    
}