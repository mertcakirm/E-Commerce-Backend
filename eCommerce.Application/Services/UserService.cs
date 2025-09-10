using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

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


    public async Task<ServiceResult<IEnumerable<UserDto>>> GetAllUsers(string token)
    {
        // Token'dan rolü al
        var role = _tokenService.GetRoleFromToken(token);

        // Eğer admin değilse erişimi reddet
        if (role != "Admin")
            return ServiceResult<IEnumerable<UserDto>>.Fail("Bu işlem için yetkiniz yok!", HttpStatusCode.Forbidden);

        // Tüm kullanıcıları getir
        var users = await _userRepository.GetAllUsers();

        // User -> UserDto dönüşümü
        var usersDto = users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role
        }).ToList();

        // Başarılı sonucu dön
        return ServiceResult<IEnumerable<UserDto>>.Success(usersDto);
    }

}