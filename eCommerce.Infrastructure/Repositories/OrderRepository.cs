using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        // Kullanıcının tüm siparişlerini getir
        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(oi=>oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId && !EF.Property<bool>(o, "IsDeleted"))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // Siparişe ait detayları getir
        public async Task<Order?> GetOrderDetailsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Payment)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && !EF.Property<bool>(o, "IsDeleted"));
        }

        // Ödeme durumunu güncelle
        public async Task UpdatePaymentStatusAsync(int orderId, string newStatus, string transactionId = null)
        {
            var order = await _dbSet
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId && !EF.Property<bool>(o, "IsDeleted"));

            if (order != null && order.Payment != null)
            {
                order.Payment.PaymentStatus = newStatus;
                if (!string.IsNullOrEmpty(transactionId))
                    order.Payment.TransactionId = transactionId;

                _dbSet.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        // Sipariş durumunu güncelle
        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _dbSet.FirstOrDefaultAsync(o => o.Id == orderId && !EF.Property<bool>(o, "IsDeleted"));
            if (order != null)
            {
                order.Status = newStatus;
                _dbSet.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        // En çok satılan ürünleri getir
        public async Task<IEnumerable<Product>> GetTopProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.Variants)
                    .ThenInclude(pv => pv.OrderItems)
                .OrderByDescending(p => p.Variants.Sum(v => v.OrderItems.Count))
                .Take(count)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int OrderId)
        {
            return await _context.Orders
                .Include(o=>o.Payment)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync( o => o.Id == OrderId && !EF.Property<bool>(o, "IsDeleted"));
                
        }
        
        public async Task CompleteOrderStatusAsync(int orderId)
        {
            var order = await _dbSet.FirstOrDefaultAsync(o => o.Id == orderId && !EF.Property<bool>(o, "IsDeleted"));
            if (order != null)
            {
                order.IsComplete = true;
                _dbSet.Update(order);
                await _context.SaveChangesAsync();
            }
        }
        
    }
}