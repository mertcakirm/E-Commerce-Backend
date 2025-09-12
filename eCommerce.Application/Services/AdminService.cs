using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IProductRepository _productRepository;

    public AdminService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IProductRepository productRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _productRepository = productRepository;
    }

    private bool IsAdmin(string token)
    {
        var role = _tokenService.GetRoleFromToken(token);
        return role == "Admin";
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetAllUsers(string token, int pageNumber, int pageSize)
    {
        if (!IsAdmin(token))
            return ServiceResult<PagedResult<UserDto>>.Fail("You do not have permission for this action!", HttpStatusCode.Forbidden);

        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

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

        var pagedResult = new PagedResult<UserDto>(pagedUsers, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<UserDto>>.Success(pagedResult, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> DeleteUser(string token, int userId)
    {
        if (!IsAdmin(token))
            return ServiceResult.Fail("You do not have permission for this action!", HttpStatusCode.Forbidden);

        var user = await _userRepository.GetByIdUser(userId);
        if (user == null)
            return ServiceResult.Fail("User not found", HttpStatusCode.NotFound);

        await _userRepository.DeleteUserAsync(userId);
        return ServiceResult.Success(HttpStatusCode.OK);
    }
    
    public async Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate)
    {
        if (!IsAdmin(token))
            return ServiceResult.Fail("You do not have permission for this action!", HttpStatusCode.Forbidden);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return ServiceResult.Fail("Product not found", HttpStatusCode.NotFound);

        await _productRepository.DiscountProductAsync(productId,discountRate);

        return ServiceResult.Success(HttpStatusCode.OK);
    }
}