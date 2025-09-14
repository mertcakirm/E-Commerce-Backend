using eCommerce.Application.DTOs;

namespace eCommerce.Application.Interfaces
{   
    public interface ICategoryService
    {
        Task<ServiceResult<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
        Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<ServiceResult<CategoryDto>> AddCategoryAsync(CategoryRequestDto categoryRequestDto, string token);
        Task<ServiceResult> DeleteCategoryAsync(int id, string token);
    }
}