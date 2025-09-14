namespace eCommerce.Application.DTOs;

public class CategoryRequestDto
{
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
}
