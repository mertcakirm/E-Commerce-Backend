using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetUserCartAsync(int userId);
        Task ClearCartAsync(int userId);
        Task AddItemAsync(int userId, int productVariantId, int quantity = 1);
        Task IncreaseItemByProductIdAsync(int userId, int variantId);
        Task DecreaseItemByProductIdAsync(int userId, int variantId);
        Task<bool> DeleteProductFromCartAsync(int cartId, int userId);
    }
}