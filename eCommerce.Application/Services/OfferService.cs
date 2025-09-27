using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.ServicesImpl;

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

    public async Task<List<Product>> GetDiscountMatchedProductsAsync(int offerId)
    {
        return await _offerRepository.GetProductsMatchingOfferDiscountAsync(offerId);
    }
}