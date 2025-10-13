using eCommerce.Core.Entities;

namespace eCommerce.Application.DTOs;

public class OrderResponseDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public bool IsComplete { get; set; }
    public string ShippingAddress { get; set; } 

    public string Status { get; set; }
    public string UserEmail { get; set; }
    
    public List<PaymentResponseDto> Payment { get; set; } = new();
    public List<OrderItemResponseDto> OrderItem { get; set; } = new();

}


public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public string PaymentMethod { get; set; } // CreditCard, Paypal, etc.
    public string PaymentStatus { get; set; } // Pending, Paid, Failed
}

public class OrderItemResponseDto
{
    public int OrderItemId  { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public List<ProductVariantOrderResponseDto> ProductVariantOrder { get; set; }
    public List<OrderItemProductResponseDto> OrderItemProduct { get; set; } = new();
    
    
}

public class OrderItemProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DiscountRate { get; set; }
    public double AverageRating { get; set; }
    public decimal Price { get; set; }
}


public class ProductVariantOrderResponseDto
{
    public string Size { get; set; }
}