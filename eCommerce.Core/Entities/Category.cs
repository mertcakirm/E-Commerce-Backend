namespace eCommerce.Core.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; }
    public Category? ParentCategory { get; set; }
    public string ImageUrl { get; set; }

    public ICollection<Category> SubCategories { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}