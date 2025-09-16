using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces;

public interface IAdminService
{
    Task<ServiceResult<PagedResult<UserDto>>> GetAllUsers(string token,int  pageNumber,int pageSize);
    Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate);
    Task<ServiceResult> DeleteUser(string token, int userId);
    Task<ServiceResult> UpdateOrderStatusAsync(int orderId, string status, string token);

}