using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces;

public interface IUserAddressService
{
    Task<ServiceResult<UserAddressDto>> CreateUserAddressAsync(UserAddressDto userAddressDto, string token);
    Task<ServiceResult<IEnumerable<UserAddressResponseDto>>> GetUserAddressesAsync(string token);
    Task<ServiceResult<UserAddressDto>> UpdateUserAddressAsync(int addressId, UserAddressDto userAddressDto,string token);
    Task<ServiceResult<bool>> DeleteUserAddressAsync(int addressId,string token);
}