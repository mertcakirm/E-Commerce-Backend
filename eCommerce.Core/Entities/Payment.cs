namespace eCommerce.Core.Entities;

public class Payment : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; }

    public string PaymentMethod { get; set; } // CreditCard, Paypal, etc.
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed
    public string TransactionId { get; set; }
}