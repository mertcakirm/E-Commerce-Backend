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
        

        // PUT: api/Orders/{orderId}/payment
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
        
        
        // PUT: api/Orders/{orderId}/payment
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
    }
}