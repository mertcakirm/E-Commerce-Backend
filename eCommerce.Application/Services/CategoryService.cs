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
        private readonly UserValidator _userValidator;
        private readonly IAuditLogService _auditLogService;

        public CategoryService(ICategoryRepository categoryRepository, UserValidator userValidator, IAuditLogService auditLogService)
        {
            _categoryRepository = categoryRepository;
            _userValidator = userValidator;
            _auditLogService = auditLogService;
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
            if (category == null) return ServiceResult<CategoryDto>.Fail("Kategori bulunamadı", HttpStatusCode.NotFound);

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
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail) return ServiceResult<CategoryDto>.Fail(validation.ErrorMessage!, validation.Status);

            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<CategoryDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
            
            var category = new Category
            {
                Name = categoryReqDto.Name,
                ParentCategoryId = categoryReqDto.ParentCategoryId
            };

            await _categoryRepository.AddAsync(category);
            await _auditLogService.LogAsync(
                userId: validation.Data!.Id,
                action: "AddCategory",
                entityName: "Category",
                entityId: null,
                details: $"Kategori eklendi: {validation.Data!.Email}"
            );
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
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);


            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return ServiceResult.Fail("Kategori bulunamadı", HttpStatusCode.NotFound);

            await _categoryRepository.RemoveAsync(category);
            await _auditLogService.LogAsync(
                userId: validation.Data!.Id,
                action: "RemoveCategory",
                entityName: "Category",
                entityId: id,
                details: $"Kategori silindi: {validation.Data!.Email}"
            );
            return ServiceResult.Success();
        }
    }
}