using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            return await _dbSet
                .Where(r => r.ProductId == productId)
                .ToListAsync();
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            return await _dbSet
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => r.Rating);
        }
    }
}