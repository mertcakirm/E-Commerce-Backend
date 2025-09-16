using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly UserValidator _userValidator;

    public AdminService(
        IUserRepository userRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        UserValidator userValidator)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _userValidator = userValidator;
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetAllUsers(string token, int pageNumber, int pageSize)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<PagedResult<UserDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

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
        return ServiceResult.Success(status: HttpStatusCode.OK);
    }

    public async Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return ServiceResult.Fail("Ürün bulunamadı!", HttpStatusCode.NotFound);

        await _productRepository.DiscountProductAsync(productId, discountRate);
        return ServiceResult.Success(status: HttpStatusCode.OK);
    }

    public async Task<ServiceResult> UpdateOrderStatusAsync(int orderId, string status, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return ServiceResult.Fail("Sipariş bulunamadı", HttpStatusCode.NotFound);

        order.Status = status;
        await _orderRepository.UpdateAsync(order);
        return ServiceResult.Success(status: HttpStatusCode.OK, message: "Sipariş durumu güncellendi!");
    }
}