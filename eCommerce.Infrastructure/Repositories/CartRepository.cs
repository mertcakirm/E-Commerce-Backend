using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using eCommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

    public async Task AddItemAsync(int userId, int productVariantId, int quantity = 1)
    {
        var cart = await _dbSet
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);


        var product = await _context.ProductVariants.Include(v=>v.Product).FirstOrDefaultAsync(p=>p.Id == productVariantId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>()
            };
            await _dbSet.AddAsync(cart);
        }

        var existingItem = cart.CartItems
            .FirstOrDefault(ci => ci.ProductVariantId == productVariantId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            _context.CartItems.Update(existingItem);
        }
        else
        {
            var newItem = new CartItem
            {
                ProductId = product.Product.Id,
                ProductVariantId = productVariantId, 
                Quantity = quantity
            };
            cart.CartItems.Add(newItem);
        }

        await _context.SaveChangesAsync();
    }

    public async Task IncreaseItemByProductIdAsync(int userId, int variantId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Cart.UserId == userId && ci.ProductVariant.Id == variantId);

        if (cartItem != null)
        {
            cartItem.Quantity += 1;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DecreaseItemByProductIdAsync(int userId, int variantId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.Cart.UserId == userId && ci.ProductVariant.Id == variantId);

        if (cartItem != null)
        {
            cartItem.Quantity -= 1;
            if (cartItem.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                _context.CartItems.Update(cartItem);
            }
            await _context.SaveChangesAsync();
        }
    }
}