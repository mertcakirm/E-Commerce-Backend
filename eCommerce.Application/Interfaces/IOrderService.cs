using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<List<OrderResponseDto>>> GetUserOrderAsync(string token);
    Task<ServiceResult<Order>> CreateOrderAsync(OrderCreateDto dto ,string token);
}