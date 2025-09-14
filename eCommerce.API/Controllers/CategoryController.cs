using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAllCategoriesAsync();
            if (result.IsFail) return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (result.IsFail) return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result.Data);
        }
        

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add([FromHeader(Name = "Authorization")] string token,[FromBody] CategoryRequestDto categoryDto)
        {
            var result = await _categoryService.AddCategoryAsync(categoryDto, token);

            if (result.IsFail) return StatusCode((int)result.Status, result.ErrorMessage);

            return Created(result.UrlAsCreated, result.Data);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromHeader(Name = "Authorization")] string token,int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id, token);

            if (result.IsFail) return StatusCode((int)result.Status, result.ErrorMessage);

            return NoContent();
        }
    }
}