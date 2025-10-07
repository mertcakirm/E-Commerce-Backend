using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count);
        Task<bool> UpdateProductSaleCount(int productId);
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword, int? categoryId);
        Task<IEnumerable<ProductVariant>> GetStockReportAsync();
        Task<IEnumerable<Product>> GetProductByCategory(string categoryName);
        Task<IEnumerable<Product>> GetAllWithDetailsAsync();
        Task<bool> DiscountProductAsync(int productId, int discountRate);
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<bool> DeleteImageAsync(int id);
        Task<ProductVariant?> GetVariantById(int variantId);
        Task<bool> AddStockAsync(int productId, string newSize, int quantity);
        Task<bool> RemoveStockAsync(int VariantId);
        Task<ProductImage> AddImageAsync(ProductImage image);
        Task<List<ProductImage>> GetImageByProductIdAsync(int productId);
        Task<List<Product>> GetProductsWithLowStockAsync(int limit);
        Task<bool> UpdateOrderStockAsync(int productVariantId, int quantity);
    }
}