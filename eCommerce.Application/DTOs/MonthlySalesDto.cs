namespace eCommerce.Application.DTOs;

public class MonthlySalesDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int SalesCount { get; set; }

    // İsteğe bağlı olarak ay adını direkt DTO içinde hesaplayabilirsin:
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));
}