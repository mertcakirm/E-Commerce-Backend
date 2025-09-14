using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count)
        {
            return await _dbSet
                .Include(p => p.OrderItems)
                .OrderByDescending(p => p.OrderItems.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string keyword, int? categoryId)
        {
            var query = _dbSet.Include(p => p.Variants).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name.Contains(keyword));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<ProductVariant>> GetStockReportAsync()
        {
            return await _context.ProductVariants
                .Include(v => v.Product)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Include(p=>p.Category)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Include(p=>p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        
        public async Task<bool> DeleteImageAsync(int id)
        {
            var image = await _context.ProductImages.FirstOrDefaultAsync(p => p.Id == id);
            if (image == null)
                return false;

            _context.ProductImages.Remove(image);
            
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
        
        public async Task<bool> DiscountProductAsync(int productId, int discountRate)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return false;

            product.DiscountRate = discountRate ;

            _context.Products.Update(product);

            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
        
        public async Task<IEnumerable<Product>> GetProductByCategory(string categoryName)
        {
            return await _dbSet
                .Where(p => p.Category.Name == categoryName)
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Include(p=>p.Category)
                .ToListAsync();
        }
        }
    }
