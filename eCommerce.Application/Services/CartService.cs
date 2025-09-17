using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Interfaces;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly UserValidator _userValidator;
    private readonly IAuditLogService _auditLogService;

    public CartService(ICartRepository cartRepository, UserValidator userValidator, IAuditLogService auditLogService)
    {
        _cartRepository = cartRepository;
        _userValidator = userValidator;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<CartResponseDto>> GetUserCartAsync(string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult<CartResponseDto>.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        var cart = await _cartRepository.GetUserCartAsync(userId);
        if (cart == null) return ServiceResult<CartResponseDto>.Fail("Sepet bulunamadı", HttpStatusCode.NotFound);

        var cartDto = new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CartItems = cart.CartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductVariantId = ci.ProductVariantId,
                Quantity = ci.Quantity,
                Product = new ProductResponseDto
                {
                    Id = ci.ProductVariant.Product.Id,
                    Name = ci.ProductVariant.Product.Name,
                    Description = ci.ProductVariant.Product.Description,
                    DiscountRate = ci.ProductVariant.Product.DiscountRate,
                    Price = ci.ProductVariant.Product.Price * (1 - (ci.ProductVariant.Product.DiscountRate / 100m )),
                    CategoryId = ci.ProductVariant.Product.CategoryId,
                    Variants = ci.ProductVariant.Product.Variants.Select(v => new ProductVariantResponseDto
                    {
                        Id = v.Id,
                        Size = v.Size,
                        Stock = v.Stock
                    }).ToList(),
                    Images = ci.ProductVariant.Product.Images.Select(i => new ProductImageResponseDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    }).ToList()
                }
            }).ToList()
        };

        return ServiceResult<CartResponseDto>.Success(cartDto);
    }
    
    public async Task<ServiceResult> AddItemAsync(string token, int productId, int productVariantId)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        if (userId == null) return ServiceResult.Fail("Geçersiz kullanıcı", HttpStatusCode.Unauthorized);

        await _cartRepository.AddItemAsync(userId, productId, productVariantId, 1);
        await _auditLogService.LogAsync(
            userId: null,
            action: "AddToCart",
            entityName: "ProductCart",
            entityId: productId,
            details: $"Ürün sepete eklendi: {validation.Data!.Email}"
        );
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ClearCartAsync(string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;
        
        await _cartRepository.ClearCartAsync(userId);
        await _auditLogService.LogAsync(
            userId: null,
            action: "ClearCart",
            entityName: "ProductCartClear",
            entityId: null,
            details: $"Sepet temizlendi: {validation.Data!.Email}"
        );
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> IncreaseItemAsync(int productId, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        var cart = await _cartRepository.GetUserCartAsync(userId);
        if (cart == null) return ServiceResult.Fail("Kart bulunamadı", HttpStatusCode.NotFound);

        if (cart.UserId != userId) return ServiceResult.Fail("Bu işlem için yetkiniz yok", HttpStatusCode.Forbidden);

        await _cartRepository.IncreaseItemByProductIdAsync(userId, productId);
        await _auditLogService.LogAsync(
            userId: null,
            action: "IncreaseCart",
            entityName: "ProductCartIncrease",
            entityId: productId,
            details: $"Sepetteki ürün sayısı arttırıldı: {validation.Data!.Email}"
        );
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DecreaseItemAsync(int productId, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        var cart = await _cartRepository.GetUserCartAsync(userId);
        if (cart == null) return ServiceResult.Fail("Kart bulunamadı", HttpStatusCode.NotFound);

        if (cart.UserId != userId) return ServiceResult.Fail("Bu işlem için yetkiniz yok", HttpStatusCode.Forbidden);

        await _cartRepository.DecreaseItemByProductIdAsync(userId, productId);
        await _auditLogService.LogAsync(
            userId: null,
            action: "DecreaseCart",
            entityName: "ProductCartDecrease",
            entityId: productId,
            details: $"Sepetteki ürün sayısı azaltıldı: {validation.Data!.Email}"
        );
        return ServiceResult.Success();
    }
}