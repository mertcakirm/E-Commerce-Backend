using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

    public interface IProductService
    {
        Task<ServiceResult<List<ProductResponseDto>>> GetAllProductsAsync();
        Task<ServiceResult<List<ProductResponseDto>>> GetProductByCategoryAsync(string categoryName);
        Task<ServiceResult<ProductResponseDto>> GetProductByIdAsync(int id);
        Task<ServiceResult<ProductDto>> CreateProductAsync(ProductDto product,string token);
        Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, ProductDto product,string token);
        Task<ServiceResult> DeleteProductAsync(int id,string token);
        Task<ServiceResult> DeleteImageAsync(int id,string token);
        
    }
