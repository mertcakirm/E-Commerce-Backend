using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System.Net;
using eCommerce.Application.DTOs;

namespace eCommerce.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly UserValidator _userValidator;
        private readonly IAuditLogService _auditLogService;

        public WishlistService(IWishlistRepository wishlistRepository, UserValidator userValidator, IAuditLogService auditLogService)
        {
            _wishlistRepository = wishlistRepository;
            _userValidator = userValidator;
            _auditLogService = auditLogService;
        }

        public async Task<ServiceResult<PagedResult<ProductResponseDto>>> GetUserWishlistAsync(string token, int pageNumber, int pageSize)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail) 
                return ServiceResult<PagedResult<ProductResponseDto>>.Fail(validation.ErrorMessage!, validation.Status);

            var userId = validation.Data!.Id;
            var allItems = await _wishlistRepository.GetUserWishlistAsync(userId);

            var totalCount = allItems.Count();
            var pagedItems = allItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var productDtos = pagedItems
                .Where(w => w.Product != null)
                .Select(w => new ProductResponseDto
                {
                    Id = w.Product!.Id,
                    Name = w.Product!.Name ?? "",
                    Description = w.Product!.Description ?? "",
                    DiscountRate = w.Product!.DiscountRate,
                    Price = w.Product!.Price,
                    AverageRating = w.Product!.AverageRating,

                    // 🔹 Çoklu kategori
                    CategoryIds = w.Product.ProductCategories?.Select(pc => pc.CategoryId).ToList() ?? new List<int>(),
                    CategoryNames = w.Product.ProductCategories?.Select(pc => pc.Category?.Name ?? "").ToList() ?? new List<string>(),

                    Variants = w.Product!.Variants?.Select(v => new ProductVariantResponseDto
                    {
                        Id = v.Id,
                        Size = v.Size,
                        Stock = v.Stock
                    }).ToList() ?? new List<ProductVariantResponseDto>(),

                    Images = w.Product!.Images?.Select(i => new ProductImageResponseDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    }).ToList() ?? new List<ProductImageResponseDto>()
                })
                .ToList();

            var pagedResult = new PagedResult<ProductResponseDto>(productDtos, totalCount, pageNumber, pageSize);

            return ServiceResult<PagedResult<ProductResponseDto>>.Success(pagedResult, status: HttpStatusCode.OK);
        }

        public async Task<ServiceResult> AddToWishlistAsync(int productId, string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

            var userId = validation.Data!.Id;
            var existingItem = await _wishlistRepository.FindAsync(w => w.UserId == userId && w.ProductId == productId);
            var wishlistItem = existingItem.FirstOrDefault();

            if (wishlistItem != null)
            {
                await _wishlistRepository.RemoveAsync(wishlistItem);
                await _wishlistRepository.SaveChangesAsync();
                await _auditLogService.LogAsync(
                    userId: userId,
                    action: "RemoveProductToWishlist",
                    entityName: "Wishlist",
                    entityId: productId,
                    details: $"Ürün favorilerden silindi: {productId}"
                );
                return ServiceResult.Success("Ürün wishlist'ten çıkarıldı", HttpStatusCode.OK);
            }
            else
            {
                wishlistItem = new Wishlist
                {
                    UserId = userId,
                    ProductId = productId
                };
                await _wishlistRepository.AddAsync(wishlistItem);
                await _wishlistRepository.SaveChangesAsync();
                await _auditLogService.LogAsync(
                    userId: userId,
                    action: "AddProductToWishlist",
                    entityName: "Wishlist",
                    entityId: productId,
                    details: $"Ürün favorilere eklendi: {productId}"
                );
                return ServiceResult.Success("Ürün wishlist'e eklendi", HttpStatusCode.OK);
            }
        }

        public async Task<ServiceResult> RemoveFromWishlistAsync(int productId, string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

            var userId = validation.Data!.Id;

            var wishlistItem = await _wishlistRepository.GetByProductIdAsync(productId);
            if (wishlistItem == null || wishlistItem.UserId != userId)
                return ServiceResult.Fail("Bu wishlist öğesine erişim yetkiniz yok", HttpStatusCode.Forbidden);

            await _wishlistRepository.RemoveAsync(wishlistItem);
            await _wishlistRepository.SaveChangesAsync();
            await _auditLogService.LogAsync(
                userId: userId,
                action: "RemoveProductToWishlist",
                entityName: "Wishlist",
                entityId: productId,
                details: $"Ürün favorilerden silindi: {productId}"
            );
            return ServiceResult.Success(status: HttpStatusCode.OK);
        }
    }
}