using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories;

public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _context;

    public OfferRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Offer?> GetByIdAsync(int offerId)
    {
        return await _context.Offers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == offerId && o.IsActive);
    }

    public async Task<List<Product>> GetProductsMatchingOfferDiscountAsync(int offerId)
    {
        // İlgili kampanyayı çek
        var offer = await GetByIdAsync(offerId);
        if (offer == null || !offer.IsActive || offer.DiscountRate == null)
            return new List<Product>();

        var discount = (int)Math.Round(offer.DiscountRate.Value);

        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.ProductOffers)
            .ThenInclude(po => po.Offer)
            .Where(p =>
                p.IsActive &&
                p.DiscountRate == discount &&
                p.ProductOffers.Any(po =>
                    po.OfferId == offerId &&
                    po.Offer.IsActive))
            .ToListAsync();

        return products;
    }
}