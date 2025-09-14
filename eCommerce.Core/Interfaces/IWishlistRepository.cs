using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IWishlistRepository : IGenericRepository<Wishlist>
    {
        Task<IEnumerable<Wishlist>> GetUserWishlistAsync(int userId);
        Task<Wishlist?> GetByProductIdAsync(int productId);
        
    }
}