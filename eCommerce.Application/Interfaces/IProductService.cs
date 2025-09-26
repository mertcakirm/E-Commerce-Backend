using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IProductService
{
    Task<ServiceResult<PagedResult<ProductResponseDto>>> GetAllProductsAsync(int pageNumber, int pageSize);
    Task<ServiceResult<PagedResult<ProductResponseDto>>> GetProductByCategoryAsync(string categoryName,int pageNumber, int pageSize);
    Task<ServiceResult<ProductResponseDto>> GetProductByIdAsync(int id);
    Task<ServiceResult<ProductDto>> CreateProductAsync(ProductCreateDto dto, string token);
    Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, UpdateProductDto product,string token);
    Task<ServiceResult> DeleteProductAsync(int id,string token);
    Task<ServiceResult> DeleteImageAsync(int id,string token);
    Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate);
    Task<ServiceResult> RemoveStock(int variantId, string token);
    Task<ServiceResult> AddStock(int productId, string newSize, int quantity, string token);
    Task<ServiceResult<ProductImage>> AddProductImageAsync(ProductImage image, string token);
}