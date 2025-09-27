using eCommerce.Application.DTOs;
using eCommerce.Core.Entities;

namespace eCommerce.Application.Interfaces;

public interface ISliderCartService
{
    Task<ServiceResult<List<SliderContentResponseDto>>> GetAllSlidersAsync();
    Task<ServiceResult<SliderContentResponseDto>> GetSliderByIdAsync(int id);
    Task<ServiceResult<SliderContentResponseDto>> AddSliderAsync(SliderContent slider,string token);
    Task<ServiceResult<bool>> DeleteSliderAsync(int id,string token);
    Task<ServiceResult<List<CartContentResponseDto>>> GetAllCartsAsync();
    Task<ServiceResult<CartContentResponseDto>> GetCartByIdAsync(int id);
    Task<ServiceResult<CartContentResponseDto>> AddCartAsync(CartContent cart,string token);
    Task<ServiceResult<bool>> DeleteCartAsync(int id,string token);


}