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
                return Ok(new { Success = true, OrderId = order.Id, TotalAmount = order.TotalAmount });
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
    }
}