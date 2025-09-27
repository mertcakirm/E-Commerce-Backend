using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(int offerId);
    Task<List<Product>> GetProductsMatchingOfferDiscountAsync(int offerId);
}