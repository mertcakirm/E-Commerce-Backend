using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

    public interface IProductService
    {
        Task<ServiceResult<List<Product>>> GetAllProductsAsync();
        Task<ServiceResult<Product>> GetProductByIdAsync(int id);
        Task<ServiceResult<Product>> CreateProductAsync(ProductDto product);
        Task<ServiceResult<Product>> UpdateProductAsync(int id, Product product);
        Task<ServiceResult> DeleteProductAsync(int id);
        
    }
