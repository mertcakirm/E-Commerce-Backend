using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context) { }

        public async Task<Cart> GetUserCartAsync(int userId)
        {
            return await _dbSet
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await GetUserCartAsync(userId);
            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
            }
        }
        

        public async Task IncreaseItemAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                item.Quantity += 1;
                _context.CartItems.Update(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DecreaseItemAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                item.Quantity -= 1;
                if (item.Quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    _context.CartItems.Update(item);
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}