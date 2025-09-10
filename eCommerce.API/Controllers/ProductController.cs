using eCommerce.Application.Interfaces;
using eCommerce.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllProductsAsync();
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto product)
        {
            var result = await _productService.CreateProductAsync(product);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Created(result.UrlAsCreated!, result);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDto product)
        {
            var result = await _productService.UpdateProductAsync(id, product);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Ok(result);
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return NoContent();
        }
    }
}