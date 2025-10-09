using eCommerce.Application.DTOs;
using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateMessage(
            [FromBody] MessageDto message,
            [FromHeader(Name = "Authorization")] string token)
        {
            var result = await _messageService.CreateMessageAsync(message,token);

            if (result.IsFail)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result);
        }

        [HttpGet("get-all")]
        [Authorize]
        public async Task<IActionResult> GetAllMessages(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(token)) return Unauthorized("Token eksik.");
            
            var result = await _messageService.GetAllMessagesAsync(pageNumber, pageSize, token);

            if (result.IsFail)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPut("reply/{messageId}")]
        [Authorize]
        public async Task<IActionResult> ToggleMessageReply(
            int messageId,
            [FromBody] string answer,
            [FromHeader(Name = "Authorization")] string token)
        {
            if (string.IsNullOrEmpty(token)) return Unauthorized("Token eksik.");
            
            var result = await _messageService.ToggleMessageReplyAsync(messageId, answer, token);

            if (result.IsFail)
                return StatusCode((int)result.Status, result.ErrorMessage);

            return Ok(result);
        }
    }
}