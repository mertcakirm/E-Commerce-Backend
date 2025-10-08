using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionAndAnswerController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionAndAnswerController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet("get-all")]
        [Authorize]
        public async Task<IActionResult> GetAllQuestions([FromHeader(Name = "Authorization")] string token)
        {
            var result = await _questionService.GetProductQuestions(token);

            if (result.IsFail)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPost("add-question")]
        [Authorize]
        public async Task<IActionResult> AddQuestion([FromHeader(Name = "Authorization")] string token, [FromBody] AddQuestionRequest request)
        {
            var result = await _questionService.AddProductQuestionAsync(request.ProductId, request.QuestionText, token);

            if (result.IsFail)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(new { message = "Soru başarıyla eklendi." });
        }

        [HttpPost("add-answer")]
        [Authorize]
        public async Task<IActionResult> AddAnswer([FromHeader(Name = "Authorization")] string token, [FromBody] AddAnswerRequest request)
        {
            var result = await _questionService.AddProductAnswerAsync(request.QuestionId, request.AnswerText, token);

            if (result.IsFail)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(new { message = "Cevap başarıyla eklendi." });
        }
        
        
        [HttpDelete("question/{questionId}")]
        public async Task<IActionResult> DeleteProductQuestionFromCart([FromHeader(Name = "Authorization")] string token,int questionId)
        {
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Token gerekli");

            var result = await _questionService.RemoveProductQuestionAsync(questionId, token);

            if (result.IsFail)
                return BadRequest(result.ErrorMessage);

            return Ok(new { success = true });
        }
    }


}