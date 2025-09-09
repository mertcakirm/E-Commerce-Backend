using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);
        Task<double> GetProductAverageRatingAsync(int productId);
    }
}