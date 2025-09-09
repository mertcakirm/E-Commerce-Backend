using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using eCommerce.Application.DTOs;

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Ok(new
            {
                Product = result.Data,
                Variants = result.Data.Variants,
                Images = result.Data.Images
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductDto product)
        {
            var result = await _productService.CreateProductAsync(product);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Created(result.UrlAsCreated!, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            var result = await _productService.UpdateProductAsync(id, product);
            if (result.IsFail)
                return StatusCode((int)result.Status, result);

            return Ok(result);
        }

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