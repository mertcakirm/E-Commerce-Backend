using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IUserService
{
    Task<ServiceResult<bool>> UpdatePassword(string token, string oldPassword, string newPassword);
    Task<ServiceResult<UserDto>> GetUserByIdAsync(string token);
}