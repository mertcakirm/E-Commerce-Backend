using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System.Net;

namespace eCommerce.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITokenService _tokenService;

        public CategoryService(ICategoryRepository categoryRepository, ITokenService tokenService)
        {
            _categoryRepository = categoryRepository;
            _tokenService = tokenService;
        }

        public async Task<ServiceResult<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllWithSubCategoriesAsync();
            var dto = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId
            });

            return ServiceResult<IEnumerable<CategoryDto>>.Success(dto);
        }

        public async Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryWithSubCategoriesAsync(id);
            if (category == null)
                return ServiceResult<CategoryDto>.Fail("Kategori bulunamadı", HttpStatusCode.NotFound);

            var dto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId
            };

            return ServiceResult<CategoryDto>.Success(dto);
        }
        

        public async Task<ServiceResult<CategoryDto>> AddCategoryAsync(CategoryRequestDto categoryReqDto, string token)
        {
            var userId = _tokenService.GetUserIdFromToken(token);

            if (userId == null || userId <= 0)
                return ServiceResult<CategoryDto>.Fail("Token geçersiz veya yetkisiz", HttpStatusCode.Unauthorized);

            var category = new Category
            {
                Name = categoryReqDto.Name,
                ParentCategoryId = categoryReqDto.ParentCategoryId
            };

            await _categoryRepository.AddAsync(category);

            var resultDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId
            };

            return ServiceResult<CategoryDto>.SuccessAsCreated(resultDto, $"/api/category/{category.Id}");
        }

        public async Task<ServiceResult> DeleteCategoryAsync(int id, string token)
        {
            var userId = _tokenService.GetUserIdFromToken(token);

            if (userId == null || userId <= 0)
                return ServiceResult.Fail("Token geçersiz veya yetkisiz", HttpStatusCode.Unauthorized);

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return ServiceResult.Fail("Kategori bulunamadı", HttpStatusCode.NotFound);

            await _categoryRepository.RemoveAsync(category);
            return ServiceResult.Success();
        }
    }
}