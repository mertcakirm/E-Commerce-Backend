using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces;

public interface IAdminService
{
    Task<ServiceResult<PagedResult<UserDto>>> GetAllUsers(string token,int  pageNumber,int pageSize);
    Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate);
}