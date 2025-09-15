using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count);
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword, int? categoryId);
        Task<IEnumerable<ProductVariant>> GetStockReportAsync();
        Task<IEnumerable<Product>> GetProductByCategory(string categoryName);
        Task<IEnumerable<Product>> GetAllWithDetailsAsync();
        Task<bool> DiscountProductAsync(int productId, int discountRate);
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<bool> DeleteImageAsync(int id);
        Task<ProductVariant?> GetVariantById(int variantId);
    }
}