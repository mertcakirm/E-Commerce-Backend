using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<bool> UpdateProductSaleCount(int productId);
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword, int? categoryId);
        Task<IEnumerable<Product>> GetProductByCategory(string categoryName);
        Task<IEnumerable<Product>> GetAllWithDetailsAsync(string? searchTerm = null);
        Task<bool> DiscountProductAsync(int productId, int discountRate);
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<bool> DeleteImageAsync(int id);
        Task<bool> AddStockAsync(int productId, string newSize, int quantity);
        Task<bool> RemoveStockAsync(int VariantId);
        Task<ProductImage> AddImageAsync(ProductImage image);
        Task<List<Product>> GetProductsWithLowStockAsync(int limit);
        Task<bool> UpdateOrderStockAsync(int productVariantId, int quantity);
        Task<bool> AddProductQuestion(int productId, string question, int userId);
        Task<bool> AddProductAnswer(int questionId, string answer, int userId);
        Task<List<ProductQuestion>> GetProductQuestions();
        Task<bool> DeleteProductQuestion(int questionId);
    }
}