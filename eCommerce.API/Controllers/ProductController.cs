using eCommerce.Application.Interfaces;
using eCommerce.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetAllProductsAsync(pageNumber, pageSize);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

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

        // POST: api/products
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromHeader(Name = "Authorization")] string token,[FromBody] ProductDto product)
        {
            var result = await _productService.CreateProductAsync(product,token);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Created(result.UrlAsCreated!, result);
        }
        
        // PUT: api/products/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromHeader(Name = "Authorization")] string token,int id, [FromBody] ProductDto product)
        {
            var result = await _productService.UpdateProductAsync(id, product,token);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Ok(result);
        }

        // DELETE: api/products/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromHeader(Name = "Authorization")] string token,int id)
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
    }
}