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

    // Admin yetkisi kontrolü için yardımcı metod
    private bool IsAdmin(string token)
    {
        var role = _tokenService.GetRoleFromToken(token);
        return role == "Admin";
    }

    // Tüm kullanıcıları sayfalı şekilde getir
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

    // Ürüne indirim uygulama (sadece admin)
    public async Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate)
    {
        if (!IsAdmin(token))
            return ServiceResult.Fail("You do not have permission for this action!", HttpStatusCode.Forbidden);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return ServiceResult.Fail("Product not found", HttpStatusCode.NotFound);

        product.Price -= product.Price * discountRate / 100;
        await _productRepository.DiscountProductAsync(productId,discountRate);

        return ServiceResult.Success(HttpStatusCode.OK);
    }
}