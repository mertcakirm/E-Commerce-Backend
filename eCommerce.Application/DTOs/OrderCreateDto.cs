namespace eCommerce.Application.DTOs;

public class OrderCreateDto
{
    public List<OrderItemDto> Items { get; set; }
    public string PaymentMethod { get; set; }
    public string ShippingAddress { get; set; } 
}

public class OrderItemDto
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
}