namespace eCommerce.Application.DTOs;

public class CartResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemDto> CartItems { get; set; }
    public List<ProductResponseDto> ProductResponseDtos { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
}