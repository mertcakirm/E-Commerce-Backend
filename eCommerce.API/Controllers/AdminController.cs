using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _env;
    public AdminController(IProductService productService, ICategoryService categoryService, IOrderService orderService, IUserService userService, IWebHostEnvironment env)
        {
        _productService = productService;
        _categoryService = categoryService;
        _orderService = orderService;
        _userService = userService;
        _env = env;
        }
    
    [Authorize]
    [HttpGet("users/get-all")]
    public async Task<IActionResult> GetAllUsers([FromHeader(Name = "Authorization")] string token,[FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token gerekli");

        var users = await _userService.GetAllUsers(token,pageNumber,pageSize);
        return Ok(users);
    }
    
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(int userId,[FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _userService.DeleteUser(token, userId);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);
    }

    [HttpPut("{orderId}/payment")]
    [Authorize]
    public async Task<IActionResult> UpdatePaymentStatus([FromHeader(Name = "Authorization")] string token,int orderId, [FromBody] PaymentUpdateDto dto)
    {
        try
        {
            await _orderService.UpdatePaymentStatusAsync(orderId, dto.Status, token);
            return Ok(new { Success = true, Message = "Ödeme durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
        
        
    [HttpPut("{orderId}/order")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus([FromHeader(Name = "Authorization")] string token,int orderId, [FromBody] PaymentUpdateDto dto)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(orderId, dto.Status, token);
            return Ok(new { Success = true, Message = "Sipariş durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
        
    [HttpPut("{orderId}/order/complete")]
    [Authorize]
    public async Task<IActionResult> CompleteOrderStatus([FromHeader(Name = "Authorization")] string token,int orderId)
    {
        try
        {
            await _orderService.CompleteOrderStatusAsync(orderId, token);
            return Ok(new { Success = true, Message = "Sipariş durumu güncellendi" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Success = false, Message = ex.Message });
        }
    }
    
    [HttpGet("notCompleted")]
    public async Task<IActionResult> GetNotCompletedOrders([FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token eksik.");

        var result = await _orderService.GetNotCompletedOrdersAsync(token);

        if (result.IsFail)
            return StatusCode((int)result.Status, new { errors = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedOrders([FromHeader(Name = "Authorization")] string token)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token eksik.");

        var result = await _orderService.GetCompletedOrdersAsync(token);

        if (result.IsFail)
            return StatusCode((int)result.Status, new { errors = result.ErrorMessage });

        return Ok(result.Data);
    }
    
    
    [Authorize]
    [HttpPost("product")]
    [RequestSizeLimit(50_000_000)] // 50 MB örnek
    public async Task<IActionResult> CreateProduct(
        [FromHeader(Name = "Authorization")] string token,
        [FromForm] ProductCreateDto product) 
    {
        var result = await _productService.CreateProductAsync(product, token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return Created(result.UrlAsCreated!, result);
    }
    
    [Authorize]
    [HttpPut("products/{productId}/discount/{discountRate}")]
    public async Task<IActionResult> DiscountProduct([FromHeader(Name = "Authorization")] string token,int productId, int discountRate)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _productService.DiscountProduct(token, productId, discountRate);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);

    }
        
    [Authorize]
    [HttpPut("product/{id}")]
    public async Task<IActionResult> UpdateProduct([FromHeader(Name = "Authorization")] string token,int id, [FromBody] UpdateProductDto product)
    {
        var result = await _productService.UpdateProductAsync(id, product,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("product/{id}")]
    public async Task<IActionResult> DeleteProduct([FromHeader(Name = "Authorization")] string token,int id)
    {
        var result = await _productService.DeleteProductAsync(id,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
        
    [Authorize]
    [HttpDelete("product/image/{id}")]
    public async Task<IActionResult> DeleteImage([FromHeader(Name = "Authorization")] string token,int id)
    {
        var result = await _productService.DeleteImageAsync(id,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
    
  [Authorize]
  [HttpPost("{productId}/upload-image")]
  [Consumes("multipart/form-data")]
  public async Task<IActionResult> UploadProductImage(
      int productId,
      [FromForm] UploadProductImageRequest request,
      [FromHeader(Name = "Authorization")] string token)
  {
      if (request.File == null || request.File.Length == 0)
          return BadRequest("Dosya geçersiz");
  
      if (string.IsNullOrEmpty(token))
          return Unauthorized("Token bulunamadı");
  
      // Dosya kaydetme işlemi
      var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
      if (!Directory.Exists(uploadsFolder))
          Directory.CreateDirectory(uploadsFolder);
  
      var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";
      var filePath = Path.Combine(uploadsFolder, fileName);
  
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
          await request.File.CopyToAsync(stream);
      }
  
      var productImage = new ProductImage
      {
          ProductId = productId,
          ImageUrl = $"/images/products/{fileName}"
      };
  
      var result = await _productService.AddProductImageAsync(productImage, token);
  
      if (!result.IsSuccess)
          return StatusCode((int)result.Status, result.ErrorMessage);
  
      return Ok(result.Data);
  }
    
    [Authorize]
    [HttpPost("product/{productId}/stock/{size}/{quantity}")]
    public async Task<IActionResult> AddStock([FromHeader(Name = "Authorization")] string token,int productId, string size, int quantity)
    {
        var result = await _productService.AddStock(productId,size,quantity,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("product/stock/{variantId}")]
    public async Task<IActionResult> RemoveStock([FromHeader(Name = "Authorization")] string token,int variantId)
    {
        var result = await _productService.RemoveStock(variantId,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
    
    [Authorize]
    [HttpPost("category")]
    public async Task<IActionResult> AddCategory(
        [FromHeader(Name = "Authorization")] string token,
        [FromForm] CategoryRequestDto categoryDto)
    {
        var result = await _categoryService.AddCategoryAsync(categoryDto, token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result.ErrorMessage);

        return Created(result.UrlAsCreated, result.Data);
    }
    
    [Authorize]
    [HttpDelete("category/{id}")]
    public async Task<IActionResult> DeleteCategory([FromHeader(Name = "Authorization")] string token,int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, token);

        if (result.IsFail) return StatusCode((int)result.Status, result.ErrorMessage);

        return NoContent();
    }
    
    
    
    
}