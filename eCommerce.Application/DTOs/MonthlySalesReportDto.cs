namespace eCommerce.Application.DTOs;

public class MonthlySalesReportDto
{
    public string ReportMonth { get; set; } 
    public decimal transferTotal { get; set; }
    public decimal CreditCartTotal { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal NetProfit { get; set; }
    public int OrdersCount { get; set; }
}