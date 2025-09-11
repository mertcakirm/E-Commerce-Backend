using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces;

public interface ICartService
{
    Task<ServiceResult<CartResponseDto>> GetUserCartAsync(string token);
    Task<ServiceResult> ClearCartAsync(string token);
    Task<ServiceResult> IncreaseItemAsync(int cartItemId, string token);
    Task<ServiceResult> DecreaseItemAsync(int cartItemId, string token);
}