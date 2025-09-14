using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(
            [FromBody] CommentCreateDto commentDto,
            [FromHeader(Name = "Authorization")] string token)
        {
            var comment = await _commentService.AddCommentAsync(commentDto, token);
            if (comment == null)
                return Unauthorized("Geçersiz kullanıcı");

            return Ok(comment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            return Ok(comment);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetCommentsByProduct(int productId)
        {
            var comments = await _commentService.GetCommentsByProductIdAsync(productId);
            return Ok(comments);
        }
        

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            [FromHeader(Name = "Authorization")] string token)
        {
            var success = await _commentService.DeleteCommentAsync(id, token);
            if (!success)
                return Forbid();

            return NoContent();
        }
    }
}