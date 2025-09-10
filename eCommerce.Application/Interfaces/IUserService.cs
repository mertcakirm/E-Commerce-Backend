using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IUserService
{
    Task<ServiceResult<IEnumerable<UserDto>>> GetAllUsers(string token);
}