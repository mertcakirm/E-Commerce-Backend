using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces;
    public interface IWishlistService
    {
        Task<ServiceResult<PagedResult<ProductResponseDto>>> GetUserWishlistAsync(string token, int pageNumber, int pageSize);
        Task<ServiceResult> AddToWishlistAsync(int productId, string token);
        Task<ServiceResult> RemoveFromWishlistAsync(int wishlistId, string token);
    }
