using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

    public interface IProductService
    {
        Task<ServiceResult<List<ProductResponseDto>>> GetAllProductsAsync();
        Task<ServiceResult<ProductResponseDto>> GetProductByIdAsync(int id);
        Task<ServiceResult<ProductDto>> CreateProductAsync(ProductDto product);
        Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, ProductDto product);
        Task<ServiceResult> DeleteProductAsync(int id);
        Task<ServiceResult> DeleteImageAsync(int id);
        
    }
