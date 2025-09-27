using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;

    public OfferService(IOfferRepository offerRepository)
    {
        _offerRepository = offerRepository;
    }

    public async Task<Offer?> GetOfferAsync(int offerId)
    {
        return await _offerRepository.GetByIdAsync(offerId);
    }

    public async Task<List<DiscountProductDto>> GetDiscountMatchedProductsAsync(int offerId)
    {
        var products = await _offerRepository.GetProductsMatchingOfferDiscountAsync(offerId);

        var result = products.Select(p => new DiscountProductDto
        {
            Id           = p.Id,
            Name         = p.Name,
            Price    = p.Price,
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
        }).ToList();

        return result;
    }
}