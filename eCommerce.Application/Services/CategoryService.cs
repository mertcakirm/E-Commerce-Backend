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
                ParentCategoryId = c.ParentCategoryId,
                ImageUrl = c.ImageUrl
                
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
                ParentCategoryId = category.ParentCategoryId,
                ImageUrl = category.ImageUrl
            };

            return ServiceResult<CategoryDto>.Success(dto);
        }
        

        public async Task<ServiceResult<CategoryDto>> AddCategoryAsync(CategoryRequestDto dto, string token)
        {
            var validation = await _userValidator.ValidateAsync(token);
            if (validation.IsFail)
                return ServiceResult<CategoryDto>.Fail(validation.ErrorMessage!, validation.Status);

            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<CategoryDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            string savedPath = null;

            // ---- Resmi Kaydet ----
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var ext = Path.GetExtension(dto.Image.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                savedPath = $"/images/categories/{fileName}"; // DB'ye bu yol kaydedilir
            }

            var category = new Category
            {
                Name = dto.Name,
                ParentCategoryId = dto.ParentCategoryId,
                ImageUrl = savedPath
            };

            await _categoryRepository.AddAsync(category);

            await _auditLogService.LogAsync(
                userId: validation.Data!.Id,
                action: "AddCategory",
                entityName: "Category",
                entityId: category.Id,
                details: $"Kategori eklendi: {category.Id}"
            );

            var resultDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId,
                ImageUrl = category.ImageUrl
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
                details: $"Kategori silindi: {id}"
            );
            return ServiceResult.Success();
        }
    }
}