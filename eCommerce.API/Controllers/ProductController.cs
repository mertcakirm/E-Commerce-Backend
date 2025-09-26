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


    }
}