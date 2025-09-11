using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IUserService
{
    Task<ServiceResult<PagedResult<UserDto>>> GetAllUsers(string token,int  pageNumber,int pageSize);
    Task<ServiceResult<bool>> UpdatePassword(string token, string oldPassword, string newPassword);
}