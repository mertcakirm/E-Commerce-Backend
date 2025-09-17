using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<IEnumerable<Product>> GetTopProductsAsync(int count);
        Task<Order?> GetOrderByIdAsync(int OrderId);
        Task<Order?> GetOrderDetailsByIdAsync(int orderId);
        Task UpdateOrderStatusAsync(int orderId, string newStatus);
        Task CompleteOrderStatusAsync(int orderId);
        Task<IEnumerable<Order>> GetNotCompletedOrdersAsync();
        Task<IEnumerable<Order>> GetCompletedOrdersAsync();
    }
}