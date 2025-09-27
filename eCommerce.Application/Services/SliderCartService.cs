using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

namespace eCommerce.Application.Services
{
    public class SliderCartService : ISliderCartService
    {
        private readonly IGenericRepository<SliderContent> _sliderRepository;
        private readonly IGenericRepository<CartContent> _cartRepository;
        private readonly UserValidator _userValidator;

        public SliderCartService(
            IGenericRepository<SliderContent> sliderRepository,
            IGenericRepository<CartContent> cartRepository
            ,UserValidator userValidator)
        {
            _sliderRepository = sliderRepository;
            _cartRepository = cartRepository;
            _userValidator = userValidator;
        }

        public async Task<ServiceResult<List<SliderContentResponseDto>>> GetAllSlidersAsync()
        {
            var sliders = await _sliderRepository.GetAllAsync();
            var dtos = sliders.Select(s => new SliderContentResponseDto
            {
                ImageUrl = s.ImageUrl,
                ParentName = s.ParentName,
                Name = s.Name,
                SubName = s.SubName,
                Href = s.Href
            }).ToList();

            return ServiceResult<List<SliderContentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<SliderContentResponseDto>> GetSliderByIdAsync(int id)
        {
            var slider = await _sliderRepository.GetByIdAsync(id);
            if (slider == null) return ServiceResult<SliderContentResponseDto>.Fail("Slider bulunamadı", HttpStatusCode.NotFound);

            var dto = new SliderContentResponseDto
            {
                ImageUrl = slider.ImageUrl,
                ParentName = slider.ParentName,
                Name = slider.Name,
                SubName = slider.SubName,
                Href = slider.Href
            };
            return ServiceResult<SliderContentResponseDto>.Success(dto);
        }

        public async Task<ServiceResult<SliderContentResponseDto>> AddSliderAsync(SliderContent slider,string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail)
                return ServiceResult<SliderContentResponseDto>.Fail(validation.ErrorMessage!, validation.Status);

            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<SliderContentResponseDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            await _sliderRepository.AddAsync(slider);
            var dto = new SliderContentResponseDto
            {
                ImageUrl = slider.ImageUrl,
                ParentName = slider.ParentName,
                Name = slider.Name,
                SubName = slider.SubName,
                Href = slider.Href
            };
            return ServiceResult<SliderContentResponseDto>.Success(dto);
        }

        public async Task<ServiceResult<bool>> DeleteSliderAsync(int id,string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail)
                return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var slider = await _sliderRepository.GetByIdAsync(id);
            if (slider == null) return ServiceResult<bool>.Fail("Slider bulunamadı", HttpStatusCode.NotFound);

            await _sliderRepository.RemoveAsync(slider);
            return ServiceResult<bool>.Success(true);
        }


  
        public async Task<ServiceResult<List<CartContentResponseDto>>> GetAllCartsAsync()
        {
            var carts = await _cartRepository.GetAllAsync();
            var dtos = carts.Select(c => new CartContentResponseDto
            {
                Name = c.Name,
                Href = c.Href,
                CartSize = c.CartSize,
                ImageUrl = c.ImageUrl
            }).ToList();

            return ServiceResult<List<CartContentResponseDto>>.Success(dtos);
        }

        public async Task<ServiceResult<CartContentResponseDto>> GetCartByIdAsync(int id)
        {
            var cart = await _cartRepository.GetByIdAsync(id);
            if (cart == null) return ServiceResult<CartContentResponseDto>.Fail("Cart bulunamadı", HttpStatusCode.NotFound);

            var dto = new CartContentResponseDto
            {
                Name = cart.Name,
                Href = cart.Href,
                CartSize = cart.CartSize,
                ImageUrl = cart.ImageUrl
            };
            return ServiceResult<CartContentResponseDto>.Success(dto);
        }

        public async Task<ServiceResult<CartContentResponseDto>> AddCartAsync(CartContent cart,string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail)
                return ServiceResult<CartContentResponseDto>.Fail(validation.ErrorMessage!, validation.Status);

            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<CartContentResponseDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
            await _cartRepository.AddAsync(cart);
            var dto = new CartContentResponseDto
            {
                Name = cart.Name,
                Href = cart.Href,
                CartSize = cart.CartSize,
                ImageUrl = cart.ImageUrl
            };
            return ServiceResult<CartContentResponseDto>.Success(dto);
        }


        public async Task<ServiceResult<bool>> DeleteCartAsync(int id,string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail)
                return ServiceResult<bool>.Fail(validation.ErrorMessage!, validation.Status);

            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var cart = await _cartRepository.GetByIdAsync(id);
            if (cart == null) return ServiceResult<bool>.Fail("Cart bulunamadı", HttpStatusCode.NotFound);

            await _cartRepository.RemoveAsync(cart);
            return ServiceResult<bool>.Success(true);
        }

     

    }
}