using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<IEnumerable<Product>> GetTopProductsAsync(int count);
    }
}