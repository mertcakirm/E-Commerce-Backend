namespace eCommerce.Application.DTOs;

public class OrderCreateDto
{
    public string PaymentMethod { get; set; }
    public int AddressId { get; set; } 
}
