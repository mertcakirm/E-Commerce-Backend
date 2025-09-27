using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IOfferService
{
    Task<Offer?> GetOfferAsync(int offerId);
    Task<List<Product>> GetDiscountMatchedProductsAsync(int offerId);
}