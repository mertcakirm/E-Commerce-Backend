using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(OrderCreateDto dto ,string token);
    Task UpdatePaymentStatusAsync(int orderId, string status, string token);
}