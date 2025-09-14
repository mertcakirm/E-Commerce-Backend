using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IUserAddressRepository
{
    Task<bool> CreateUserAddressAsync(UserAddress userAddress);
    Task<IEnumerable<UserAddress>> GetUserAddressAll(int userId);
    Task<bool> DeleteUserAddressAsync(int AddressId);
    Task<bool> UpdateUserAddressAsync(UserAddress userAddress,int AddressId);
    Task<UserAddress?> GetByIdAsync(int addressId);
}