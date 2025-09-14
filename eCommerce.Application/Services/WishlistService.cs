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
        private readonly ITokenService _tokenService;

        public WishlistService(IWishlistRepository wishlistRepository, ITokenService tokenService)
        {
            _wishlistRepository = wishlistRepository;
            _tokenService = tokenService;
        }

        public async Task<ServiceResult<PagedResult<ProductResponseDto>>> GetUserWishlistAsync(string token, int pageNumber, int pageSize)
        {
            var userId = _tokenService.GetUserIdFromToken(token);
            if (userId == null)
                return ServiceResult<PagedResult<ProductResponseDto>>.Fail("Geçersiz token", HttpStatusCode.Unauthorized);

            var allItems = await _wishlistRepository.GetUserWishlistAsync(userId);

            var totalCount = allItems.Count();
            var pagedItems = allItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Wishlist -> ProductResponseDto dönüştür
            var productDtos = pagedItems.Select(w => new ProductResponseDto
            {
                Id = w.Product.Id,
                Name = w.Product.Name,
                Description = w.Product.Description,
                DiscountRate = w.Product.DiscountRate,
                BasePrice = w.Product.BasePrice,
                Price = w.Product.Price,
                AverageRating = w.Product.AverageRating,
                CategoryId = w.Product.CategoryId,
                Variants = w.Product.Variants.Select(v => new ProductVariantResponseDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Stock = v.Stock
                }).ToList(),
                Images = w.Product.Images.Select(i => new ProductImageResponseDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            }).ToList();

            var pagedResult = new PagedResult<ProductResponseDto>(productDtos, totalCount, pageNumber, pageSize);

            return ServiceResult<PagedResult<ProductResponseDto>>.Success(pagedResult, HttpStatusCode.OK);
        }

        public async Task<ServiceResult> AddToWishlistAsync(int productId, string token)
        {
            var userId = _tokenService.GetUserIdFromToken(token);
            if (userId == null)
                return ServiceResult.Fail("Geçersiz token", HttpStatusCode.Unauthorized);

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductId = productId
            };

            await _wishlistRepository.AddAsync(wishlistItem);
            await _wishlistRepository.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.OK);
        }

        public async Task<ServiceResult> RemoveFromWishlistAsync(int wishlistId, string token)
        {
            var userId = _tokenService.GetUserIdFromToken(token);
            if (userId == null)
                return ServiceResult.Fail("Geçersiz token", HttpStatusCode.Unauthorized);

            var wishlistItem = await _wishlistRepository.GetByIdAsync(wishlistId);
            if (wishlistItem == null || wishlistItem.UserId != userId)
                return ServiceResult.Fail("Bu wishlist öğesine erişim yetkiniz yok", HttpStatusCode.Forbidden);

            await _wishlistRepository.RemoveAsync(wishlistItem);
            await _wishlistRepository.SaveChangesAsync();

            return ServiceResult.Success(HttpStatusCode.OK);
        }
    }
}