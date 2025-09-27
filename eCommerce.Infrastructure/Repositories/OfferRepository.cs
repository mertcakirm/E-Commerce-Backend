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
            .FirstOrDefaultAsync(o => o.Id == offerId);
    }
    
    public async Task<List<Product>> GetProductsByDiscountAsync(decimal discountRate)
    {
        return await _context.Products
            .Where(p => p.IsActive && p.DiscountRate == discountRate)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .ToListAsync();
    }

    public async Task<List<Offer>> GetAllAsync()
    {
        return await _context.Offers.Where(o=>!o.IsDeleted).ToListAsync();
    }
    
    public async Task<Offer> CreateOfferAsync(Offer offer)
    {
        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();
        return offer;
    }
    
    public async Task<bool> RemoveOfferAsync(int  offerId)
    {
        var offer = await _context.Offers.FirstOrDefaultAsync(o=>o.Id == offerId);
        
        if(offer == null) return false;
        
        offer.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> ToggleOfferAsync(int offerId)
    {
        var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == offerId);

        if (offer == null) return false;

        offer.IsActive = !offer.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }
    
}