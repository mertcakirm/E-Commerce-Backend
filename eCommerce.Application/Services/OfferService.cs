using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

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

    public async Task<List<ServiceResult<DiscountProductDto>>> GetDiscountMatchedProductsAsync(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId);

        if (offer == null || offer.DiscountRate == null)
            return new List<ServiceResult<DiscountProductDto>>();

        var discount = offer.DiscountRate.Value;

        var products = await _offerRepository.GetProductsByDiscountAsync(discount);

        var result = products.Select(p => new DiscountProductDto
            {
                Id           = p.Id,
                Name         = p.Name,
                Price        = p.Price,
                DiscountRate = p.DiscountRate,
                ImageUrls    = p.Images?
                    .Where(i => i != null)
                    .Select(i => i!.ImageUrl)
                    .ToList() ?? new List<string>(),
                Variants     = p.Variants?
                    .Select(v => new ProductVariantResponseDto
                    {
                        Id    = v.Id,
                        Size  = v.Size,
                        Stock = v.Stock
                    }).ToList() ?? new List<ProductVariantResponseDto>()
            }).Select(p => ServiceResult<DiscountProductDto>.Success(p)) 
            .ToList();

        return result;
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