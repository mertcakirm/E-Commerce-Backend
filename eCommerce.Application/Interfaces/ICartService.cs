using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces;

public interface ICartService
{
    Task<ServiceResult<CartResponseDto>> GetUserCartAsync(string token);
    Task<ServiceResult> AddItemAsync(string token, int productVariantId);
    Task<ServiceResult> ClearCartAsync(string token);
    Task<ServiceResult> IncreaseItemAsync(int variantId, string token);
    Task<ServiceResult> DecreaseItemAsync(int variantId, string token);
    Task<ServiceResult<bool>> DeleteProductFromCartAsync(int cartId, string token);
}