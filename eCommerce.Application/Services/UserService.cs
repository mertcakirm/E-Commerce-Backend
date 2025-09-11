using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
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


    public async Task<ServiceResult<PagedResult<UserDto>>> GetAllUsers(string token, int pageNumber, int pageSize)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var role = _tokenService.GetRoleFromToken(token);

        if (role != "Admin")
            return ServiceResult<PagedResult<UserDto>>.Fail("Bu işlem için yetkiniz yok!", HttpStatusCode.Forbidden);

        var users = await _userRepository.GetAllUsers();

        var usersDto = users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role
        }).ToList();

        var totalCount = usersDto.Count;

        var pagedUsers = usersDto
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // İşte parametreli constructor kullanımı
        var pagedResult = new PagedResult<UserDto>(
            pagedUsers,  // items
            totalCount,  // totalCount
            pageNumber,  // pageNumber
            pageSize     // pageSize
        );

        return ServiceResult<PagedResult<UserDto>>.Success(pagedResult, HttpStatusCode.OK);
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
    
    

}