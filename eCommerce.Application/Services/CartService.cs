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

    public CartService(ICartRepository cartRepository, UserValidator userValidator)
    {
        _cartRepository = cartRepository;
        _userValidator = userValidator;
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
                ProductVariantId = ci.ProductVariant.Id,
                ProductVariantName = ci.ProductVariant.Size,
                Quantity = ci.Quantity,
                Product = new ProductResponseDto
                {
                    Id = ci.ProductVariant.Product.Id,
                    Name = ci.ProductVariant.Product.Name,
                    Description = ci.ProductVariant.Product.Description,
                    DiscountRate = ci.ProductVariant.Product.DiscountRate,
                    Price = ci.ProductVariant.Product.Price,
                    PriceWithDiscount = ci.ProductVariant.Product.Price * (1 - (ci.ProductVariant.Product.DiscountRate / 100m )),
                    CategoryId = ci.ProductVariant.Product.CategoryId,
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
    
    public async Task<ServiceResult> AddItemAsync(string token, int productVariantId)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        if (userId == null) return ServiceResult.Fail("Geçersiz kullanıcı", HttpStatusCode.Unauthorized);

        await _cartRepository.AddItemAsync(userId, productVariantId, 1);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ClearCartAsync(string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;
        
        await _cartRepository.ClearCartAsync(userId);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> IncreaseItemAsync(int variantId, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        var cart = await _cartRepository.GetUserCartAsync(userId);
        if (cart == null) return ServiceResult.Fail("Kart bulunamadı", HttpStatusCode.NotFound);

        if (cart.UserId != userId) return ServiceResult.Fail("Bu işlem için yetkiniz yok", HttpStatusCode.Forbidden);

        await _cartRepository.IncreaseItemByProductIdAsync(userId, variantId);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DecreaseItemAsync(int variantId, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var userId = validation.Data!.Id;

        var cart = await _cartRepository.GetUserCartAsync(userId);
        if (cart == null) return ServiceResult.Fail("Kart bulunamadı", HttpStatusCode.NotFound);

        if (cart.UserId != userId) return ServiceResult.Fail("Bu işlem için yetkiniz yok", HttpStatusCode.Forbidden);

        await _cartRepository.DecreaseItemByProductIdAsync(userId, variantId);
        return ServiceResult.Success();
    }


    public async Task<ServiceResult<bool>> DeleteProductFromCartAsync(int cartId, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) 
            return ServiceResult<bool>.Fail("Geçersiz token");

        var userId = validation.Data!.Id;

        var cart = await _cartRepository.GetUserCartAsync(userId);
        if (cart == null) 
            return ServiceResult<bool>.Fail("Sepet bulunamadı");

        if (cart.UserId != userId) 
            return ServiceResult<bool>.Fail("Kullanıcı yetkisiz");

        var result = await _cartRepository.DeleteProductFromCartAsync(cartId, userId);

        if (!result) 
            return ServiceResult<bool>.Fail("Ürün sepetten silinemedi");

        return ServiceResult<bool>.Success(true);
    }
}