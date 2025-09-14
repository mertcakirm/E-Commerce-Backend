using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

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

    public async Task<Order> CreateOrderAsync(OrderCreateDto dto,string token)
    {
        var user = await _userRepo.GetByIdUser(dto.UserId);
        var userId = _tokenService.GetUserIdFromToken(token);
        
        if (user == null || dto.UserId != userId) throw new Exception("Kullanıcı bulunamadı");

        var order = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            OrderItems = new List<OrderItem>(),
            TotalAmount = 0
        };

        foreach (var item in dto.Items)
        {
            var variant = await _productRepo.GetByIdAsync(item.ProductVariantId);
            if (variant == null) throw new Exception("Ürün bulunamadı");

            var orderItem = new OrderItem
            {
                ProductVariantId = variant.Id,
                Quantity = item.Quantity,
                Price = variant.Price
            };

            order.OrderItems.Add(orderItem);
            order.TotalAmount += orderItem.Price * orderItem.Quantity;
        }

        // Ödeme bilgisi oluştur
        order.Payment = new Payment
        {
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = "Pending"
        };

        await _orderRepo.AddAsync(order);
        return order;
    }

    public async Task UpdatePaymentStatusAsync(int orderId, string status, string token, string transactionId = null )
    {
        
        var userId = _tokenService.GetUserIdFromToken(token);
        var order = await _orderRepo.GetByIdAsync(orderId);
        
        if(userId != order.UserId) throw new Exception("Kullanıcı bulunamadı");
        
        if (order == null || order.Payment == null) throw new Exception("Sipariş bulunamadı");

        order.Payment.PaymentStatus = status;
        if (!string.IsNullOrEmpty(transactionId)) order.Payment.TransactionId = transactionId;

        await _orderRepo.UpdateAsync(order);
    }


}