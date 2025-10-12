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
    private readonly IUserAddressRepository _userAddressRepository;
    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo,UserValidator userValidator,ICartRepository cartRepo,IAuditLogService auditLogService, IUserAddressRepository userAddressRepository)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _userValidator = userValidator;
        _cartRepo = cartRepo;
        _auditLogService = auditLogService;
        _userAddressRepository = userAddressRepository;
    }

    public async Task<ServiceResult<List<OrderResponseDto>>> GetUserOrderAsync(string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail)
            return ServiceResult<List<OrderResponseDto>>.Fail(validation.ErrorMessage!, validation.Status);

        var orders = await _orderRepo.GetUserOrdersAsync(validation.Data!.Id);
        if (!orders.Any())
            return ServiceResult<List<OrderResponseDto>>.Fail("Kullanıcının siparişi bulunamadı", HttpStatusCode.NotFound);

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
                OrderItemId = i.Id,
                Price = i.Price,
                Quantity = i.Quantity,
                ProductVariantOrder = i.ProductVariant != null
                    ? new List<ProductVariantOrderResponseDto> { new ProductVariantOrderResponseDto { Size = i.ProductVariant.Size } }
                    : new List<ProductVariantOrderResponseDto>(),
                OrderItemProduct = i.ProductVariant?.Product != null
                    ? i.ProductVariant.Product.ProductCategories.Select(pc => new OrderItemProductResponseDto
                    {
                        Id = i.ProductVariant.ProductId,
                        Name = i.ProductVariant.Product.Name,
                        Description = i.ProductVariant.Product.Description,
                        DiscountRate = i.ProductVariant.Product.DiscountRate,
                        AverageRating = i.ProductVariant.Product.AverageRating,
                        CategoryName = pc.Category.Name,
                        Price = i.ProductVariant.Product.Price
                    }).ToList()
                    : new List<OrderItemProductResponseDto>()
            }).ToList()
        }).ToList();

        return ServiceResult<List<OrderResponseDto>>.Success(orderDtos);
    }

    public async Task<ServiceResult<Order>> CreateOrderAsync(OrderCreateDto dto, string token)
    {
        var validation = await _userValidator.ValidateAsync(token);
        if (validation.IsFail)
            return ServiceResult<Order>.Fail(validation.ErrorMessage!, validation.Status);

        var user = validation.Data!;
        var address = await _userAddressRepository.GetByIdAsync(dto.AddressId);
        if (address == null || address.UserId != user.Id)
            return ServiceResult<Order>.Fail("Adres bulunamadı, sipariş oluşturulamaz.", HttpStatusCode.BadRequest);

        var joinAddress = $"{address.AddressLine} {address.City} {address.PostalCode}";
        var cart = await _cartRepo.GetUserCartAsync(user.Id);
        if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            return ServiceResult<Order>.Fail("Sepetiniz boş, sipariş oluşturulamaz.", HttpStatusCode.BadRequest);

        var order = new Order
        {
            UserId = user.Id,
            OrderDate = DateTime.UtcNow,
            Status = "Onaylandı",
            ShippingAddress = joinAddress,
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var cartItem in cart.CartItems)
        {
            var product = await _productRepo.GetByIdWithDetailsAsync(cartItem.ProductId);
            if (product == null)
                return ServiceResult<Order>.Fail($"Ürün bilgisi cartItem {cartItem.Id} için bulunamadı.", HttpStatusCode.NotFound);

            await _productRepo.UpdateOrderStockAsync(cartItem.ProductVariantId, cartItem.Quantity);
            await _productRepo.UpdateProductSaleCount(product.Id);

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
            return ServiceResult<Order>.Fail(ex.InnerException?.Message ?? ex.Message);
        }

        await _auditLogService.LogAsync(
            userId: user.Id,
            action: "CreateOrder",
            entityName: "Order",
            entityId: order.Id,
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
            details: $"Ödeme durumu güncellendi: {orderId}"
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
            details: $"Sipariş durumu oluşturuldu: {orderId}"
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
            details: $"Sipariş tamamlandı: {orderId}"
        );
        return ServiceResult.Success("Sipariş tamamlandı!");
    }

public async Task<ServiceResult<PagedResult<OrderResponseDto>>> GetNotCompletedOrdersAsync(string token, int pageNumber, int pageSize)
{
    var isAdmin = await _userValidator.IsAdminAsync(token);
    if (isAdmin.IsFail || !isAdmin.Data)
        return ServiceResult<PagedResult<OrderResponseDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

    var orders = await _orderRepo.GetNotCompletedOrdersAsync();
    if (!orders.Any())
        return ServiceResult<PagedResult<OrderResponseDto>>.Fail("Tamamlanmamış sipariş bulunamadı", HttpStatusCode.NotFound);

    var totalCount = orders.Count();
    var pagedOrders = orders.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

    var dtoList = pagedOrders.Select(o => new OrderResponseDto
    {
        Id = o.Id,
        OrderDate = o.OrderDate,
        IsComplete = o.IsComplete,
        TotalAmount = o.TotalAmount,
        ShippingAddress = o.ShippingAddress,
        Status = o.Status,
        UserEmail = o.User?.Email ?? string.Empty,
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
            OrderItemId = i.Id,
            Price = i.Price,
            Quantity = i.Quantity,
            ProductVariantOrder = i.ProductVariant != null
                ? new List<ProductVariantOrderResponseDto> { new ProductVariantOrderResponseDto { Size = i.ProductVariant.Size } }
                : new List<ProductVariantOrderResponseDto>(),
            OrderItemProduct = i.ProductVariant?.Product != null
                ? i.ProductVariant.Product.ProductCategories.Select(pc => new OrderItemProductResponseDto
                {
                    Id = i.ProductVariant.ProductId,
                    Name = i.ProductVariant.Product.Name,
                    Description = i.ProductVariant.Product.Description,
                    DiscountRate = i.ProductVariant.Product.DiscountRate,
                    AverageRating = i.ProductVariant.Product.AverageRating,
                    CategoryName = pc.Category.Name,
                    Price = i.ProductVariant.Product.Price
                }).ToList()
                : new List<OrderItemProductResponseDto>()
        }).ToList()
    }).ToList();

    var pagedResult = new PagedResult<OrderResponseDto>(dtoList, totalCount, pageNumber, pageSize);
    return ServiceResult<PagedResult<OrderResponseDto>>.Success(pagedResult);
}

public async Task<ServiceResult<PagedResult<OrderResponseDto>>> GetCompletedOrdersAsync(string token, int pageNumber, int pageSize)
{
    var isAdmin = await _userValidator.IsAdminAsync(token);
    if (isAdmin.IsFail || !isAdmin.Data)
        return ServiceResult<PagedResult<OrderResponseDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

    var orders = await _orderRepo.GetCompletedOrdersAsync();
    if (!orders.Any())
        return ServiceResult<PagedResult<OrderResponseDto>>.Fail("Tamamlanmış sipariş bulunamadı", HttpStatusCode.NotFound);

    var totalCount = orders.Count();
    var pagedOrders = orders.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

    var dtoList = pagedOrders.Select(o => new OrderResponseDto
    {
        Id = o.Id,
        OrderDate = o.OrderDate,
        IsComplete = o.IsComplete,
        TotalAmount = o.TotalAmount,
        ShippingAddress = o.ShippingAddress,
        Status = o.Status,
        UserEmail = o.User?.Email ?? string.Empty,
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
            OrderItemId = i.Id,
            Price = i.Price,
            Quantity = i.Quantity,
            ProductVariantOrder = i.ProductVariant != null
                ? new List<ProductVariantOrderResponseDto> { new ProductVariantOrderResponseDto { Size = i.ProductVariant.Size } }
                : new List<ProductVariantOrderResponseDto>(),
            OrderItemProduct = i.ProductVariant?.Product != null
                ? i.ProductVariant.Product.ProductCategories.Select(pc => new OrderItemProductResponseDto
                {
                    Id = i.ProductVariant.ProductId,
                    Name = i.ProductVariant.Product.Name,
                    Description = i.ProductVariant.Product.Description,
                    DiscountRate = i.ProductVariant.Product.DiscountRate,
                    AverageRating = i.ProductVariant.Product.AverageRating,
                    CategoryName = pc.Category.Name,
                    Price = i.ProductVariant.Product.Price
                }).ToList()
                : new List<OrderItemProductResponseDto>()
        }).ToList()
    }).ToList();

    var pagedResult = new PagedResult<OrderResponseDto>(dtoList, totalCount, pageNumber, pageSize);
    return ServiceResult<PagedResult<OrderResponseDto>>.Success(pagedResult);
}

public async Task<ServiceResult<List<MonthlyCategorySalesDto>>> GetMonthlyCategorySalesAsync(string token)
{
    var isAdmin = await _userValidator.IsAdminAsync(token);
    if (isAdmin.IsFail || !isAdmin.Data)
        return ServiceResult<List<MonthlyCategorySalesDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

    var orders = await _orderRepo.GetOrdersFromLastMonthByCategoryAsync();

    var result = orders
        .SelectMany(o => o.OrderItems)
        .SelectMany(oi => oi.ProductVariant.Product.ProductCategories, (oi, pc) => new { oi, pc })
        .GroupBy(x => new
        {
            Year = x.oi.Order.OrderDate.Year,
            Month = x.oi.Order.OrderDate.Month,
            CategoryId = x.pc.Category.Id,
            CategoryName = x.pc.Category.Name
        })
        .Select(g => new MonthlyCategorySalesDto
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            CategoryId = g.Key.CategoryId,
            CategoryName = g.Key.CategoryName,
            OrderCount = g.Count()
        })
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Month)
        .ThenBy(x => x.CategoryId)
        .ToList();

    return ServiceResult<List<MonthlyCategorySalesDto>>.Success(result);
}

public async Task<ServiceResult<List<MonthlyCategorySalesDto>>> GetYearlyCategorySalesAsync(string token)
{
    var isAdmin = await _userValidator.IsAdminAsync(token);
    if (isAdmin.IsFail || !isAdmin.Data)
        return ServiceResult<List<MonthlyCategorySalesDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

    var orders = await _orderRepo.GetOrdersGeneralByCategoryAsync();

    var result = orders
        .SelectMany(o => o.OrderItems)
        .SelectMany(oi => oi.ProductVariant.Product.ProductCategories, (oi, pc) => new { oi, pc })
        .GroupBy(x => new
        {
            CategoryId = x.pc.Category.Id,
            CategoryName = x.pc.Category.Name
        })
        .Select(g => new MonthlyCategorySalesDto
        {
            Year = 0,
            Month = 0,
            CategoryId = g.Key.CategoryId,
            CategoryName = g.Key.CategoryName,
            OrderCount = g.Count()
        })
        .OrderBy(x => x.CategoryId)
        .ToList();

    return ServiceResult<List<MonthlyCategorySalesDto>>.Success(result);
}

public async Task<ServiceResult<List<MonthlySalesDto>>> GetYearlySalesByMonthAsync(string token)
{
    try
    {
        var isAdmin = await _userValidator.IsAdminAsync(token);
        if (isAdmin.IsFail || !isAdmin.Data)
            return ServiceResult<List<MonthlySalesDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
        
        var orders = await _orderRepo.GetOrdersFromLastYearAsync();

        var groupedData = orders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlySalesDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                SalesCount = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();

        return ServiceResult<List<MonthlySalesDto>>.Success(groupedData);
    }
    catch (Exception ex)
    {
        return ServiceResult<List<MonthlySalesDto>>.Fail(ex.Message);
    }
    
}


}