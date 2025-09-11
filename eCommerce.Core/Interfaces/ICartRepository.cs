using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetUserCartAsync(int userId);
        Task ClearCartAsync(int userId);
        Task IncreaseItemAsync(int cartItemId);
        Task DecreaseItemAsync(int cartItemId);
    }
}