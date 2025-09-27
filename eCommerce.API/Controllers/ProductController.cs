using eCommerce.Application.Interfaces;
using eCommerce.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetAllProductsAsync(pageNumber, pageSize);
            if (result.IsFail) return StatusCode((int)result.Status, result);

            return Ok(result);
        }
        
        // GET: api/products/get-by-category/t-shirt?pageNumber=1&pageSize=10
        [HttpGet("get-by-category/{categoryName}")]
        public async Task<IActionResult> GetByCategory(
            string categoryName,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(categoryName))
                return Unauthorized("Kategori ismi alınamadı!");

            var result = await _productService.GetProductByCategoryAsync( categoryName,pageNumber, pageSize);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Ok(result);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Ok(result); 
        }

    [Authorize]
    [HttpPost("add")]
    [RequestSizeLimit(50_000_000)]
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
    [HttpPut("{productId}/discount/{discountRate}")]
    public async Task<IActionResult> DiscountProduct([FromHeader(Name = "Authorization")] string token,int productId, int discountRate)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token missing");

        var result = await _productService.DiscountProduct(token, productId, discountRate);
        return StatusCode((int)result.Status, result.IsFail ? result.ErrorMessage : null);

    }
        
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct([FromHeader(Name = "Authorization")] string token,int id, [FromBody] UpdateProductDto product)
    {
        var result = await _productService.UpdateProductAsync(id, product,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct([FromHeader(Name = "Authorization")] string token,int id)
    {
        var result = await _productService.DeleteProductAsync(id,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
        
    [Authorize]
    [HttpDelete("image/{id}")]
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
    [HttpPost("{productId}/stock/{size}/{quantity}")]
    public async Task<IActionResult> AddStock([FromHeader(Name = "Authorization")] string token,int productId, string size, int quantity)
    {
        var result = await _productService.AddStock(productId,size,quantity,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("stock/{variantId}")]
    public async Task<IActionResult> RemoveStock([FromHeader(Name = "Authorization")] string token,int variantId)
    {
        var result = await _productService.RemoveStock(variantId,token);
        if (result.IsFail)
            return StatusCode((int)result.Status, result);

        return NoContent();
    }


    }
}