using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count);
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword, int? categoryId);
        Task<IEnumerable<ProductVariant>> GetStockReportAsync();
    }
}