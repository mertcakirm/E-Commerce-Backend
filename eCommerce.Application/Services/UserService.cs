using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Core.Helpers;

namespace eCommerce.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    
    public UserService(IUserRepository userRepository, ITokenService tokenService)
        {
        _userRepository = userRepository;
        _tokenService = tokenService;
        }
   
    
    public async Task<ServiceResult<bool>> UpdatePassword(string token, string oldPassword, string newPassword)
    {
        int userId;
        var newHashPassword = CreatePassword.CreatePasswordHash(newPassword);
        var oldhashPassword = CreatePassword.CreatePasswordHash(oldPassword);
        try
        {
            userId = _tokenService.GetUserIdFromToken(token);
        }
        catch
        {
            return ServiceResult<bool>.Fail("Geçersiz token", HttpStatusCode.Unauthorized);
        }

        var userExists = await _userRepository.IsUser(userId);
        if (!userExists)
            return ServiceResult<bool>.Fail("Kullanıcı bulunamadı", HttpStatusCode.NotFound);

        var user = (await _userRepository.GetAllUsers()).FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return ServiceResult<bool>.Fail("Kullanıcı bulunamadı", HttpStatusCode.NotFound);

        if (user.PasswordHash != oldhashPassword) 
            return ServiceResult<bool>.Fail("Eski şifre yanlış", HttpStatusCode.BadRequest);

        var updated = await _userRepository.UpdatePassword(userId, newHashPassword);
        if (!updated)
            return ServiceResult<bool>.Fail("Şifre güncellenirken hata oluştu", HttpStatusCode.InternalServerError);

        return ServiceResult<bool>.Success(true, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UserDto>> GetProductByIdAsync(string token)
    {
        int userId;
        try
        {
            userId = _tokenService.GetUserIdFromToken(token);
        }
        catch
        {
            return ServiceResult<UserDto>.Fail("Geçersiz kullanıcı", HttpStatusCode.Unauthorized);
        }

        var user = await _userRepository.GetByIdUser(userId);
        if (user == null)
        {
            return ServiceResult<UserDto>.Fail("Kullanıcı bulunamadı", HttpStatusCode.NotFound);
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };

        return ServiceResult<UserDto>.Success(userDto, HttpStatusCode.OK);
    }
    
    

}