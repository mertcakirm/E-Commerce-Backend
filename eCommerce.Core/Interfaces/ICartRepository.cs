using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetUserCartAsync(int userId);
        Task ClearCartAsync(int userId);
        Task AddItemAsync(int userId,int productId, int productVariantId, int quantity = 1);
        Task IncreaseItemByProductIdAsync(int userId, int productId);
        Task DecreaseItemByProductIdAsync(int userId, int productId);
    }
}