using System.Net;
using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Helpers;
using eCommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Application.Services;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly UserValidator _userValidator;
    private readonly ICartRepository _cartRepo;
    private readonly IAuditLogService _auditLogService;
    
    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo,UserValidator userValidator,ICartRepository cartRepo,IAuditLogService auditLogService)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _userValidator = userValidator;
        _cartRepo = cartRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<List<OrderResponseDto>>> GetUserOrderAsync(string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail) return ServiceResult<List<OrderResponseDto>>.Fail(validation.ErrorMessage!, validation.Status);

        var orders = await _orderRepo.GetUserOrdersAsync(validation.Data!.Id);

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
    IsComplete = o.IsComplete,
    TotalAmount = o.TotalAmount,
    ShippingAddress = o.ShippingAddress,
    Status = o.Status,
    UserEmail = o.User.Email,
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
        OrderItemId = i.Id, // buraya OrderItemId atıyoruz
        Price = i.Price,
        Quantity = i.Quantity,
        
        // Eğer ProductVariantOrder dönmesi gerekiyorsa:
        ProductVariantOrder = (i.ProductVariant != null)
            ? new List<ProductVariantOrderResponseDto>
            {
                new ProductVariantOrderResponseDto
                {
                    Size = i.ProductVariant.Size
                }
            }
            : new List<ProductVariantOrderResponseDto>(),

        OrderItemProduct = (i.ProductVariant?.Product != null)
            ? new List<OrderItemProductResponseDto>
            {
                new OrderItemProductResponseDto
                {
                    Id = i.ProductVariant.ProductId, 
                    Name = i.ProductVariant.Product.Name,
                    Description = i.ProductVariant.Product.Description,
                    DiscountRate = i.ProductVariant.Product.DiscountRate,
                    AverageRating = i.ProductVariant.Product.AverageRating,
                    CategoryName = i.ProductVariant.Product.Category?.Name ?? "",
                    Price = i.ProductVariant.Product.Price
                }
            }
            : new List<OrderItemProductResponseDto>()
    }).ToList()
}).ToList();

        return new ServiceResult<List<OrderResponseDto>>
        {
            Data = orderDtos,
            Status = HttpStatusCode.OK
        };
    }
    
public async Task<ServiceResult<Order>> CreateOrderAsync(OrderCreateDto dto, string token)
{
    var validation = await _userValidator.ValidateAsync(token);
    if (validation.IsFail)
        return ServiceResult<Order>.Fail(validation.ErrorMessage!, validation.Status);

    var user = validation.Data!;

    var cart = await _cartRepo.GetUserCartAsync(user.Id);
    if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
        return ServiceResult<Order>.Fail("Sepetiniz boş, sipariş oluşturulamaz.", HttpStatusCode.BadRequest);

    var order = new Order
    {
        UserId = user.Id,
        OrderDate = DateTime.UtcNow,
        Status = "Pending",
        ShippingAddress = dto.ShippingAddress,
        OrderItems = new List<OrderItem>()
    };

    decimal totalAmount = 0;

    foreach (var cartItem in cart.CartItems)
    {
        var product = await _productRepo.GetByIdWithDetailsAsync(cartItem.ProductId);
        if (product == null)
            return ServiceResult<Order>.Fail($"Ürün bilgisi cartItem {cartItem.Id} için bulunamadı.", HttpStatusCode.NotFound);
        
        decimal discountedPrice = product.Price * (1 - (product.DiscountRate / 100m));

        order.OrderItems.Add(new OrderItem
        {
            ProductVariantId = cartItem.ProductVariantId,
            Quantity = cartItem.Quantity,
            Price = discountedPrice * cartItem.Quantity,
            ProductId = cartItem.ProductId
            
        });

        totalAmount += discountedPrice * cartItem.Quantity;
    }
    order.TotalAmount = totalAmount;

    order.Payment = new Payment
    {
        PaymentMethod = dto.PaymentMethod ?? throw new Exception("Payment method boş olamaz"),
        PaymentStatus = "Pending",
        TransactionId = IdHelper.GenerateRandomAlphaNumeric(9)
    };
    try
    {
        await _orderRepo.AddAsync(order);
        cart.CartItems.Clear();
        await _cartRepo.UpdateAsync(cart);
    }
    catch (DbUpdateException ex)
    {
        throw new Exception($"EF SaveChanges hatası: {ex.InnerException?.Message ?? ex.Message}");
    }
    
    await _auditLogService.LogAsync(
        userId: user.Id,
        action: "CreateOrder",
        entityName: "Order",
        entityId: null,
        details: $"Sipariş oluşturuldu: {user.Email}"
    );

    return ServiceResult<Order>.Success(order);
}

  public async Task<ServiceResult> UpdatePaymentStatusAsync(int orderId, string status, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var validation = await _userValidator.ValidateAsync(token);
        
        var userId = validation.Data!.Id;
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var order = await _orderRepo.GetOrderByIdAsync(orderId);

        if (order == null) return ServiceResult.Fail("Sipariş bulunamadı", HttpStatusCode.NotFound);

        if (order.UserId != userId) return ServiceResult.Fail("Kullanıcı yetkisiz", HttpStatusCode.Unauthorized);

        if (order.Payment == null) return ServiceResult.Fail("Ödeme bilgisi bulunamadı", HttpStatusCode.BadRequest);

        order.Payment.PaymentStatus = status;
        await _orderRepo.UpdateAsync(order);
        await _auditLogService.LogAsync(
            userId: userId,
            action: "UpdatePaymentStatus",
            entityName: "Payment",
            entityId: orderId,
            details: $"Ödeme durumu güncellendi: {validation.Data!.Email}"
        );

        return ServiceResult.Success("Ödeme durumu güncellendi");
    }
    
    public async Task<ServiceResult> UpdateOrderStatusAsync(int orderId, string status, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var validation = await _userValidator.ValidateAsync(token);
        
        var userId = validation.Data!.Id;
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var order = await _orderRepo.GetOrderByIdAsync(orderId);

        if (order == null) return ServiceResult.Fail("Sipariş bulunamadı", HttpStatusCode.NotFound);

        if (order.UserId != userId) return ServiceResult.Fail("Kullanıcı yetkisiz", HttpStatusCode.Unauthorized);

        if (order.Payment == null) return ServiceResult.Fail("Sipariş bilgisi bulunamadı", HttpStatusCode.BadRequest);

        order.Payment.PaymentStatus = status;
        await _orderRepo.UpdateOrderStatusAsync(orderId, status);
        await _auditLogService.LogAsync(
            userId: userId,
            action: "UpdateOrderStatus",
            entityName: "Order",
            entityId: orderId,
            details: $"Sipariş durumu oluşturuldu: {validation.Data!.Email}"
        );

        return ServiceResult.Success("Sipariş durumu güncellendi");
    }
    
    public async Task<ServiceResult> CompleteOrderStatusAsync(int orderId, string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        var validation = await _userValidator.ValidateAsync(token);
        
        var userId = validation.Data!.Id;
        
        if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        if (validation.IsFail) return ServiceResult.Fail(validation.ErrorMessage!, validation.Status);

        var order = await _orderRepo.GetOrderByIdAsync(orderId);

        if (order == null) return ServiceResult.Fail("Sipariş bulunamadı", HttpStatusCode.NotFound);

        if (order.UserId != userId) return ServiceResult.Fail("Kullanıcı yetkisiz", HttpStatusCode.Unauthorized);

        if (order.Payment == null) return ServiceResult.Fail("Ödeme bilgisi bulunamadı", HttpStatusCode.NotFound);

        await _orderRepo.CompleteOrderStatusAsync(orderId);
        await _auditLogService.LogAsync(
            userId: userId,
            action: "CompleteOrderStatus",
            entityName: "Order",
            entityId: orderId,
            details: $"Sipariş tamamlandı: {validation.Data!.Email}"
        );
        return ServiceResult.Success("Sipariş tamamlandı!");
    }

    public async Task<ServiceResult<List<OrderResponseDto>>> GetNotCompletedOrdersAsync(string token)
    {
        // Admin kontrolü
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<List<OrderResponseDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var orders = await _orderRepo.GetNotCompletedOrdersAsync();
        if (!orders.Any())
            return ServiceResult<List<OrderResponseDto>>.Fail("Tamamlanmamış sipariş bulunamadı", HttpStatusCode.NotFound);

       var dtoList = orders.Select(o => new OrderResponseDto
        {
            Id          = o.Id,
            OrderDate   = o.OrderDate,
            IsComplete  = o.IsComplete,
            TotalAmount = o.TotalAmount,
            ShippingAddress = o.ShippingAddress,
            Status      = o.Status,
            UserEmail   = o.User.Email,

            Payment = o.Payment != null
                ? new List<PaymentResponseDto>
                  {
                      new PaymentResponseDto
                      {
                          PaymentId     = o.Payment.Id,
                          PaymentMethod = o.Payment.PaymentMethod,
                          PaymentStatus = o.Payment.PaymentStatus
                      }
                  }
                : new List<PaymentResponseDto>(),

            OrderItem = (o.OrderItems ?? new List<OrderItem>())
                .Select(i => new OrderItemResponseDto
                {
                    OrderItemId = i.Id,
                    Price       = i.Price,
                    Quantity    = i.Quantity,

                    ProductVariantOrder = i.ProductVariant == null
                        ? new List<ProductVariantOrderResponseDto>()
                        : new List<ProductVariantOrderResponseDto>
                        {
                            new ProductVariantOrderResponseDto
                            {
                                Size = i.ProductVariant.Size
                            }
                        },
                    OrderItemProduct = new List<OrderItemProductResponseDto>
                    {
                        new OrderItemProductResponseDto
                        {
                            Id = i.ProductId,
                            Name         = i.ProductVariant?.Product?.Name ?? "",
                            Description  = i.ProductVariant?.Product?.Description ?? "",
                            DiscountRate = i.ProductVariant?.Product?.DiscountRate ?? 0,
                            AverageRating= i.ProductVariant?.Product?.AverageRating ?? 0,
                            CategoryName = i.ProductVariant?.Product?.Category?.Name ?? "",
                            Price        = i.ProductVariant?.Product?.Price ?? 0
                        }
                    }
                })
                .ToList()
        }).ToList();

        return ServiceResult<List<OrderResponseDto>>.Success(dtoList);
    }

    public async Task<ServiceResult<List<OrderResponseDto>>> GetCompletedOrdersAsync(string token)
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<List<OrderResponseDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

        var orders = await _orderRepo.GetCompletedOrdersAsync();
        if (!orders.Any())
            return ServiceResult<List<OrderResponseDto>>.Fail("Tamamlanmış sipariş bulunamadı", HttpStatusCode.NotFound);

        var dtoList = orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            IsComplete = o.IsComplete,
            TotalAmount = o.TotalAmount,
            ShippingAddress = o.ShippingAddress,
            UserEmail = o.User.Email,
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

        return ServiceResult<List<OrderResponseDto>>.Success(dtoList);
    }



}