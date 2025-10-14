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
    private readonly IAuditLogService _auditLogService;
    private readonly UserValidator _userValidator;
    
    public UserService(IUserRepository userRepository, ITokenService tokenService, IAuditLogService auditLogService, UserValidator userValidator)
        {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _auditLogService = auditLogService;
        _userValidator = userValidator;
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

        var user = await _userRepository.GetByIdUser(userId);
        if (user == null)
            return ServiceResult<bool>.Fail("Kullanıcı bulunamadı", HttpStatusCode.NotFound);

        if (user.PasswordHash != oldhashPassword) 
            return ServiceResult<bool>.Fail("Eski şifre yanlış", HttpStatusCode.BadRequest);

        var updated = await _userRepository.UpdatePassword(userId, newHashPassword);
        if (!updated)
            return ServiceResult<bool>.Fail("Şifre güncellenirken hata oluştu", HttpStatusCode.InternalServerError);

        await _auditLogService.LogAsync(
            userId: userId,
            action: "UpdatePassword",
            entityName: "User",
            entityId: null,
            details: $"Şifre güncellendi: {user.Email}"
        );
        return ServiceResult<bool>.Success(true, status:HttpStatusCode.OK);
    }

    public async Task<ServiceResult<UserDto>> GetUserByIdAsync(string token)
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

        return ServiceResult<UserDto>.Success(userDto, status:HttpStatusCode.OK);
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetAllUsers(
        string token, int pageNumber, int pageSize, string? searchTerm = null)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<PagedResult<UserDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        var users = await _userRepository.GetAllUsers(searchTerm);

        var usersDto = users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            AcceptEmails = u.AcceptEmails,
            Role = u.Role,
            IsDeleted = u.IsDeleted
        }).ToList();

        var totalCount = usersDto.Count;

        var pagedUsers = usersDto
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pagedResult = new PagedResult<UserDto>(pagedUsers, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<UserDto>>.Success(pagedResult, status: HttpStatusCode.OK);
    }

    public async Task<ServiceResult> DeleteUser(string token, int userId)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var user = await _userRepository.GetByIdUser(userId);
        if (user == null) return ServiceResult.Fail("Kullanıcı bulunamadı", HttpStatusCode.NotFound);

        await _userRepository.DeleteUserAsync(userId);
        await _auditLogService.LogAsync(
            userId: null,
            action: "DeleteUser",
            entityName: "User",
            entityId: userId,
            details: $"Kullanıcı silindi: {user.Email}"
        );
        
        return ServiceResult.Success(status: HttpStatusCode.OK);
    }

    public async Task<ServiceResult> UpdateUserStatus(int userId, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        await _userRepository.UpdateUserStatusAsync(userId);
        await _auditLogService.LogAsync(
            userId: userId,
            action: "UserStatusUpdate",
            entityName: "User",
            entityId: userId,
            details: $"Kullanıcı durumu güncellendi : {userId}"
        );
        
        return ServiceResult.Success(status: HttpStatusCode.OK);
    }

}