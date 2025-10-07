using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditService;

    public AuditLogController(IAuditLogService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(
        [FromHeader(Name = "Authorization")] string token,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _auditService.GetAllAsync(token, pageNumber, pageSize);
        return Ok(result);
    }
    
    [HttpGet("not-seen-all")]
    [Authorize]
    public async Task<IActionResult> GetAllNotSeen(
        [FromHeader(Name = "Authorization")] string token,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _auditService.GetNotSeenAllAsync(token, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPatch("toggle-see/{id}")]
    [Authorize]
    public async Task<IActionResult> ToggleSee([FromHeader(Name = "Authorization")] string token,int id)
    {
        if (string.IsNullOrEmpty(token))
            return Unauthorized("Token bulunamadı!");

        var result = await _auditService.ToggleSeeLogAsync(id, token);

        if (result.IsFail)
            return StatusCode((int)result.Status, result.ErrorMessage);

        return Ok(new
        {
            success = true,
            message = result.ErrorMessage
        });
    }
    
    
    [HttpDelete("clear")]
    [Authorize]
    public async Task<IActionResult> Clear([FromHeader(Name = "Authorization")] string token)
    {
        var logs = await _auditService.ClearAuditLogsHistoryAsync(token);
        return Ok(logs);
    }
}