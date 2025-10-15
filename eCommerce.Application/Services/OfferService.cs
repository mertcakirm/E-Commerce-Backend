using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Application.Services;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly UserValidator _userValidator;
    private readonly IAuditLogService _auditLogService;

    public OfferService(IOfferRepository offerRepository,UserValidator userValidator, IAuditLogService auditLogService)
    {
        _offerRepository = offerRepository;
        _userValidator = userValidator;
        _auditLogService = auditLogService;
    }


    public async Task<ServiceResult<List<Offer>>> GetAllAsync()
    {
        var offers = await _offerRepository.GetAllAsync();
        return ServiceResult<List<Offer>>.Success(offers);
    }

    public async Task<ServiceResult<Offer>> GetOfferAsync(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId);

        if (offer == null)
            return ServiceResult<Offer>.Fail("Kampanya bulunamadı", HttpStatusCode.NotFound);

        return ServiceResult<Offer>.Success(offer);
    }

public async Task<ServiceResult<OfferResponseDto>> GetDiscountMatchedProductsAsync(int offerId, int pageNumber, int pageSize)
{
    var offer = await _offerRepository.GetByIdAsync(offerId);

    if (offer == null || offer.DiscountRate == null)
        return ServiceResult<OfferResponseDto>.Fail("Geçersiz kampanya veya indirim oranı bulunamadı.");

    var discount = offer.DiscountRate.Value;

    var productsQuery = _offerRepository.GetProductsByDiscountQuery(discount);

    var totalCount = await productsQuery.CountAsync();

    var pagedProducts = await productsQuery
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Include(p => p.Variants)
        .Include(p => p.Images)
        .ToListAsync();

    var productDtos = pagedProducts.Select(p => new ProductResponseDto()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        DiscountRate = p.DiscountRate,
        SaleCount = p.SaleCount,
        AverageRating = p.AverageRating,
        Price = p.Price,
        PriceWithDiscount = p.Price * (1 - (p.DiscountRate / 100m)),

        CategoryIds = p.ProductCategories != null
            ? p.ProductCategories.Select(pc => pc.Category.Id).ToList()
            : new List<int>(),

        CategoryNames = p.ProductCategories != null
            ? p.ProductCategories.Select(pc => pc.Category.Name).ToList()
            : new List<string>(),

        Variants = p.Variants != null
            ? p.Variants.Select(v => new ProductVariantResponseDto
            {
                Id = v.Id,
                Size = v.Size,
                Stock = v.Stock
            }).ToList()
            : new List<ProductVariantResponseDto>(),

        Images = p.Images != null
            ? p.Images.Select(i => new ProductImageResponseDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsMain = i.IsMain
            }).ToList()
            : new List<ProductImageResponseDto>()
    }).ToList();

    var offerResponse = new OfferResponseDto
    {
        Id = offer.Id,
        OfferName = offer.Name,
        Products = productDtos
    };

    return ServiceResult<OfferResponseDto>.Success(offerResponse);
}
    
    public async Task<ServiceResult<Offer>> CreateOfferAsync(CreateOfferDto dto, string wwwRootPath,string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<Offer>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        string imageUrl = string.Empty;

        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
            var folderPath = Path.Combine(wwwRootPath, "offers");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ImageFile.CopyToAsync(stream);
            }

            // URL formatı: /offers/filename.jpg
            imageUrl = $"/offers/{fileName}";
        }

        var offer = new Offer
        {
            Name         = dto.Name,
            Description  = dto.Description,
            ImageUrl     = imageUrl,
            DiscountRate = dto.DiscountRate,
            StartDate    = dto.StartDate,
            EndDate      = dto.EndDate,
            IsActive     = true
        };

         var offerRes = await _offerRepository.CreateOfferAsync(offer);
         await _auditLogService.LogAsync(
             userId: null,
             action: "AddOffer",
             entityName: "Offer",
             entityId: null,
             details: $"Kampanya eklendi: {dto.Name}"
         );

        return ServiceResult<Offer>.Success(offerRes);
    }

    public async Task<ServiceResult> DeleteOfferAsync(int offerId,string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var offer = await _offerRepository.GetByIdAsync(offerId);
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        if (offer == null) return ServiceResult.Fail("Kampanya bulunamadı", HttpStatusCode.NotFound);

        await _offerRepository.RemoveOfferAsync(offerId);
        await _auditLogService.LogAsync(
            userId: null,
            action: "DeleteOffer",
            entityName: "Offer",
            entityId: offerId,
            details: $"Kampanya silindi: {offerId}"
        );
        return ServiceResult.Success();
    }
    
    public async Task<ServiceResult> ToggleOfferAsync(int offerId,string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var offer = await _offerRepository.GetByIdAsync(offerId);
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        if (offer == null) return ServiceResult.Fail("Kampanya bulunamadı", HttpStatusCode.NotFound);

        await _offerRepository.ToggleOfferAsync(offerId);
        await _auditLogService.LogAsync(
            userId: null,
            action: "ToggleOffer",
            entityName: "Offer",
            entityId: offerId,
            details: $"Kampanya aktiflik durumu güncellendi: {offerId}"
        );
        return ServiceResult.Success();
    }
}