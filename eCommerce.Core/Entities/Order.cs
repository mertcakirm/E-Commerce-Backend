namespace eCommerce.Core.Entities;

public class Order : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } 
    public bool IsComplete { get; set; } = false;
    public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered, Cancelled

    public ICollection<OrderItem> OrderItems { get; set; }
    public Payment Payment { get; set; }
}