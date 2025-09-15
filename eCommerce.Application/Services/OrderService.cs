using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IUserRepository _userRepo;
    private readonly ITokenService _tokenService;

    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo, IUserRepository userRepo,ITokenService tokenService)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _userRepo = userRepo;
        _tokenService = tokenService;
    }

    public async Task<Order> CreateOrderAsync(OrderCreateDto dto, string token)
    {
        var userId = _tokenService.GetUserIdFromToken(token);
        var user = await _userRepo.GetByIdUser(userId);

        if (user == null)
            throw new Exception("Kullanıcı bulunamadı");

        var order = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;
        
        foreach (var item in dto.Items)
        {
            var variant = await _productRepo.GetVariantById(item.ProductVariantId);
            if (variant == null)
                throw new Exception($"Ürün varyantı {item.ProductVariantId} bulunamadı");

            var orderItem = new OrderItem
            {
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                Price = (variant.Product.Price * (1 - (variant.Product.DiscountRate / 100m )))
                
                
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.Price * orderItem.Quantity;
        }

        order.TotalAmount = totalAmount;

        // Payment ekleniyor
        order.Payment = new Payment
        {
            PaymentMethod = dto.PaymentMethod ?? throw new Exception("Payment method boş olamaz"),
            PaymentStatus = "Pending",
            TransactionId = IdHelper.GenerateRandomAlphaNumeric(9)
        };

        try
        {
            await _orderRepo.AddAsync(order);
        }
        catch (DbUpdateException ex)
        {
            throw new Exception($"EF SaveChanges hatası: {ex.InnerException?.Message ?? ex.Message}");
        }

        return order;
    }

    public async Task UpdatePaymentStatusAsync(int orderId, string status, string token )
    {
        
        var userId = _tokenService.GetUserIdFromToken(token);
        var order = await _orderRepo.GetOrderByIdAsync(orderId);
        
        if(userId != order.UserId) throw new Exception("Kullanıcı bulunamadı");
        
        if (order == null || order.Payment == null) throw new Exception("Sipariş bulunamadı");

        order.Payment.PaymentStatus = status;

        await _orderRepo.UpdateAsync(order);
    }


}