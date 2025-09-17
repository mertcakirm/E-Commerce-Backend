using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
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
    public async Task<IActionResult> GetAll()
    {
        var logs = await _auditService.GetAllAsync();
        return Ok(logs);
    }

    [HttpPost]
    public async Task<IActionResult> Log([FromBody] AuditLog log)
    {
        await _auditService.LogAsync(log.UserId, log.Action, log.EntityName, log.EntityId, log.Details);
        return Ok(new { message = "Kayıt Eklendi!" });
    }
}