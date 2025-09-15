using System.Net;
using eCommerce.Application;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Helpers;
using eCommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public async Task<ServiceResult<List<OrderResponseDto>>> GetUserOrderAsync(string token)
    {
        var userId = _tokenService.GetUserIdFromToken(token);
        var user = await _userRepo.GetByIdUser(userId);

        if (user == null)
            return new ServiceResult<List<OrderResponseDto>>
            {
                ErrorMessage = new List<string> { "Kullanıcı bulunamadı" },
                Status = HttpStatusCode.NotFound
            };

        var orders = await _orderRepo.GetUserOrdersAsync(userId);

        var lastOrder = orders
            .OrderByDescending(o => o.OrderDate)
            .FirstOrDefault();

        if (lastOrder == null)
            return new ServiceResult<List<OrderResponseDto>>
            {
                ErrorMessage = new List<string> { "Kullanıcının siparişi bulunamadı" },
                Status = HttpStatusCode.NotFound
            };


        var orderDtos = orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            Payment = o.Payment != null 
                ? new List<PaymentResponseDto>
                {
                    new PaymentResponseDto
                    {
                        PaymentId = o.Payment.Id,
                        PaymentMethod = o.Payment.PaymentMethod,
                        PaymentStatus = o.Payment.PaymentStatus
                    }
                }
                : new List<PaymentResponseDto>(),

            OrderItem = (o.OrderItems ?? new List<OrderItem>()).Select(i => new OrderItemResponseDto
            {
                Price = i.Price,
                Quantity = i.Quantity,
                OrderItemProduct = new List<OrderItemProductResponseDto>
                {
                    new OrderItemProductResponseDto
                    {
                        Name = i.ProductVariant?.Product?.Name ?? "",
                        Description = i.ProductVariant?.Product?.Description ?? "",
                        DiscountRate = i.ProductVariant?.Product?.DiscountRate ?? 0,
                        AverageRating = i.ProductVariant?.Product?.AverageRating ?? 0,
                        CategoryName = i.ProductVariant?.Product?.Category?.Name ?? "",
                        Price = i.ProductVariant?.Product?.Price ?? 0
                    }
                }
            }).ToList()
        }).ToList();

        return new ServiceResult<List<OrderResponseDto>>
        {
            Data = orderDtos,
            Status = HttpStatusCode.OK
        };
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