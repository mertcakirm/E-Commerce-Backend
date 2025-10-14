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
        private readonly IAuditLogService _auditLogService;

        public SliderCartService(
            IGenericRepository<SliderContent> sliderRepository,
            IGenericRepository<CartContent> cartRepository
            ,UserValidator userValidator, 
            IAuditLogService auditLogService)
        {
            _sliderRepository = sliderRepository;
            _cartRepository = cartRepository;
            _userValidator = userValidator;
            _auditLogService = auditLogService;
        }

        public async Task<ServiceResult<List<SliderContentResponseDto>>> GetAllSlidersAsync()
        {
            var sliders = await _sliderRepository.GetAllAsync();
            var dtos = sliders.Select(s => new SliderContentResponseDto
            {
                Id = s.Id,
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
                Id = slider.Id,
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
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<SliderContentResponseDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            await _sliderRepository.AddAsync(slider);
            await _auditLogService.LogAsync(
                userId: null,
                action: "AddSlider",
                entityName: "SliderCart",
                entityId: null,
                details: $"Slider eklendi: {slider.Id}"
            );
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
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var slider = await _sliderRepository.GetByIdAsync(id);
            if (slider == null) return ServiceResult<bool>.Fail("Slider bulunamadı", HttpStatusCode.NotFound);

            await _sliderRepository.RemoveAsync(slider);
            await _auditLogService.LogAsync(
                userId: null,
                action: "DeleteSlider",
                entityName: "SliderCart",
                entityId: id,
                details: $"Slider silindi: {id}"
            );
            return ServiceResult<bool>.Success(true);
        }


  
        public async Task<ServiceResult<List<CartContentResponseDto>>> GetAllCartsAsync()
        {
            var carts = await _cartRepository.GetAllAsync();
            var dtos = carts.Select(c => new CartContentResponseDto
            {
                Id = c.Id,
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
                Id = cart.Id,
                Name = cart.Name,
                Href = cart.Href,
                CartSize = cart.CartSize,
                ImageUrl = cart.ImageUrl
            };
            return ServiceResult<CartContentResponseDto>.Success(dto);
        }

        public async Task<ServiceResult<CartContentResponseDto>> AddCartAsync(CartContent cart,string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<CartContentResponseDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
            await _cartRepository.AddAsync(cart);
            await _auditLogService.LogAsync(
                userId: null,
                action: "AddCart",
                entityName: "SliderCart",
                entityId: cart.Id,
                details: $"Kart eklendi: {cart.Id}"
            );
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
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<bool>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var cart = await _cartRepository.GetByIdAsync(id);
            if (cart == null) return ServiceResult<bool>.Fail("Cart bulunamadı", HttpStatusCode.NotFound);

            await _cartRepository.RemoveAsync(cart);
            await _auditLogService.LogAsync(
                userId: null,
                action: "DeleteCart",
                entityName: "SliderCart",
                entityId: id,
                details: $"Kart silindi: {id}"
            );
            return ServiceResult<bool>.Success(true);
        }

     

    }
}