using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditRepo;

    public AuditLogService(IAuditLogRepository auditRepo)
    {
        _auditRepo = auditRepo;
    }

    public async Task LogAsync(int? userId, string action, string entityName, int? entityId, string details)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details
        };

        await _auditRepo.AddAsync(log);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _auditRepo.GetAllAsync();
    }
}