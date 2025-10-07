namespace eCommerce.Application.DTOs;

public class MonthlyCategorySalesDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
}