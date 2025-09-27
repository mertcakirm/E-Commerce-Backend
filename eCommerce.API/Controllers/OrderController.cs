using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> CreateOrder([FromHeader(Name = "Authorization")] string token,[FromBody] OrderCreateDto dto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(dto, token);
                return Ok(new { Success = true, OrderId = order.Data.Id, TotalAmount = order.Data.TotalAmount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyOrders([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var order = await _orderService.GetUserOrderAsync(token);
                return Ok(new { Success = true, data = order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
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
        
        
    [HttpPut("{orderId}/status/{status}")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus([FromHeader(Name = "Authorization")] string token,int orderId,string status)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId,status, token);
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
    
    [HttpGet("notCompleted")]
    public async Task<IActionResult> GetNotCompletedOrders([FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token eksik.");

        var result = await _orderService.GetNotCompletedOrdersAsync(token);

        if (result.IsFail)
            return StatusCode((int)result.Status, new { errors = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedOrders([FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token eksik.");

        var result = await _orderService.GetCompletedOrdersAsync(token);

        if (result.IsFail)
            return StatusCode((int)result.Status, new { errors = result.ErrorMessage });

        return Ok(result.Data);
    }
        


    }
}