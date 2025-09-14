using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces;


    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category> GetCategoryWithSubCategoriesAsync(int categoryId);
        Task<IEnumerable<Category>> GetAllWithSubCategoriesAsync();
    }
