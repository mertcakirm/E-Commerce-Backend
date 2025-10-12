using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface IOfferService
{
    Task<ServiceResult<List<Offer>>> GetAllAsync();
    Task<ServiceResult<Offer>> GetOfferAsync(int offerId);
    Task<ServiceResult<PagedResult<DiscountProductDto>>> GetDiscountMatchedProductsAsync(int offerId, int pageNumber, int pageSize);
    Task<ServiceResult<Offer>> CreateOfferAsync(CreateOfferDto dto, string wwwRootPath, string token);
    Task<ServiceResult> DeleteOfferAsync(int offerId,string token);
    Task<ServiceResult> ToggleOfferAsync(int offerId,string token);
}