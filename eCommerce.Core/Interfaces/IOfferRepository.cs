using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;

public interface IOfferRepository
{
    Task<Offer?> GetByIdAsync(int offerId);
    IQueryable<Product> GetProductsByDiscountQuery(decimal discountRate);
    Task<List<Offer>> GetAllAsync();
    Task<Offer> CreateOfferAsync(Offer offer);
    Task<bool> RemoveOfferAsync(int offerId);
    Task<bool> ToggleOfferAsync(int offerId);
}