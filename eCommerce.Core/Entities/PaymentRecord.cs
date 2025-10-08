namespace eCommerce.Core.Entities;

public class PaymentRecord : BaseEntity
{
    public DateTime ReportMonth {get; set;}
    public decimal TransferTotal {get; set;}
    public decimal CreditCartTotal  {get; set;}
    public decimal TotalAmount  {get; set;}
    public decimal NetProfit {get; set;}
    public int OrdersCount {get; set;}
}