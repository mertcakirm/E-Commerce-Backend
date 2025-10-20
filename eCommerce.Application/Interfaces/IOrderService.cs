using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IOrderService
{
    Task<ServiceResult<PagedResult<OrderResponseDto>>> GetUserOrderAsync(string token, int pageNumber, int pageSize);
    Task<ServiceResult<Order>> CreateOrderAsync(OrderCreateDto dto ,string token);
    Task<ServiceResult> UpdateOrderStatusAsync(int orderId, string status, string token);
    Task<ServiceResult> UpdatePaymentStatusAsync(int orderId, string status, string token);
    Task<ServiceResult> CompleteOrderStatusAsync(int orderId, string token);
    Task<ServiceResult<PagedResult<OrderResponseDto>>> GetNotCompletedOrdersAsync(string token, int pageNumber, int pageSize);
    Task<ServiceResult<OrderResponseDto>> GetOrderByIdAsync(string token, int orderId);
    Task<ServiceResult<PagedResult<OrderResponseDto>>> GetCompletedOrdersAsync(string token, int pageNumber, int pageSize);
    Task<ServiceResult<List<MonthlySalesDto>>> GetYearlySalesByMonthAsync(string token);
    Task<ServiceResult<List<MonthlyCategorySalesDto>>> GetMonthlyCategorySalesAsync(string token);
    Task<ServiceResult<List<MonthlyCategorySalesDto>>> GetYearlyCategorySalesAsync(string token);

}