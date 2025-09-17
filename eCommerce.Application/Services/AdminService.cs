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
    private readonly IAuditLogService _auditLogService;

    public AdminService(
        IUserRepository userRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        UserValidator userValidator,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _userValidator = userValidator;
        _auditLogService = auditLogService;
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
        await _auditLogService.LogAsync(
            userId: null,
            action: "DeleteUser",
            entityName: "User",
            entityId: userId,
            details: $"Kullanıcı silindi: {user.Email}"
        );
        
        return ServiceResult.Success(status: HttpStatusCode.OK);
    }

    public async Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var user = await _userValidator.ValidateAsync(token);
        
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return ServiceResult.Fail("Ürün bulunamadı!", HttpStatusCode.NotFound);

        await _productRepository.DiscountProductAsync(productId, discountRate);
        await _auditLogService.LogAsync(
            userId: null,
            action: "DiscountProduct",
            entityName: "Product",
            entityId: productId,
            details: $"Ürüne indirim yapıldı: {user.Data!.Email}"
        );
        return ServiceResult.Success(status: HttpStatusCode.OK);
    }
    
    public async Task<ServiceResult> UpdatePaymentStatusAsync(int orderId, string status, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var validation = await _userValidator.ValidateAsync(token);
        
        var userId = validation.Data!.Id;
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order == null) return ServiceResult.Fail("Sipariş bulunamadı", HttpStatusCode.NotFound);

        if (order.UserId != userId) return ServiceResult.Fail("Kullanıcı yetkisiz", HttpStatusCode.Unauthorized);

        if (order.Payment == null) return ServiceResult.Fail("Ödeme bilgisi bulunamadı", HttpStatusCode.BadRequest);

        order.Payment.PaymentStatus = status;
        await _orderRepository.UpdateAsync(order);
        await _auditLogService.LogAsync(
            userId: userId,
            action: "UpdatePaymentStatus",
            entityName: "Payment",
            entityId: orderId,
            details: $"Ödeme durumu güncellendi: {validation.Data!.Email}"
        );

        return ServiceResult.Success("Ödeme durumu güncellendi");
    }
    
    public async Task<ServiceResult> UpdateOrderStatusAsync(int orderId, string status, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var validation = await _userValidator.ValidateAsync(token);
        
        var userId = validation.Data!.Id;
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order == null) return ServiceResult.Fail("Sipariş bulunamadı", HttpStatusCode.NotFound);

        if (order.UserId != userId) return ServiceResult.Fail("Kullanıcı yetkisiz", HttpStatusCode.Unauthorized);

        if (order.Payment == null) return ServiceResult.Fail("Sipariş bilgisi bulunamadı", HttpStatusCode.BadRequest);

        order.Payment.PaymentStatus = status;
        await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        await _auditLogService.LogAsync(
            userId: userId,
            action: "UpdateOrderStatus",
            entityName: "Order",
            entityId: orderId,
            details: $"Sipariş durumu oluşturuldu: {validation.Data!.Email}"
        );

        return ServiceResult.Success("Sipariş durumu güncellendi");
    }
    
    public async Task<ServiceResult> CompleteOrderStatusAsync(int orderId, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var validation = await _userValidator.ValidateAsync(token);
        
        var userId = validation.Data!.Id;
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var order = await _orderRepository.GetOrderByIdAsync(orderId);

        if (order == null) return ServiceResult.Fail("Sipariş bulunamadı", HttpStatusCode.NotFound);

        if (order.UserId != userId) return ServiceResult.Fail("Kullanıcı yetkisiz", HttpStatusCode.Unauthorized);

        if (order.Payment == null) return ServiceResult.Fail("Ödeme bilgisi bulunamadı", HttpStatusCode.NotFound);

        await _orderRepository.CompleteOrderStatusAsync(orderId);
        await _auditLogService.LogAsync(
            userId: userId,
            action: "CompleteOrderStatus",
            entityName: "Order",
            entityId: orderId,
            details: $"Sipariş tamamlandı: {validation.Data!.Email}"
        );
        return ServiceResult.Success("Sipariş tamamlandı!");
    }
}